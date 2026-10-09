using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Media;

namespace RemoteToolsTabbed
{
    /// <summary>
    /// A TabControl that keeps the content of every tab alive.
    ///
    /// The stock WPF TabControl swaps one ContentPresenter between tabs, so leaving a tab
    /// unloads its content and returning to it builds the content again from the template.
    /// That is fatal here: the content of a session tab is a live window belonging to
    /// another process, and rebuilding it would destroy the host and drop the session.
    ///
    /// So the template holds a Grid (PART_ItemsHolder) with one ContentPresenter per tab,
    /// created once and then only shown or hidden.
    /// </summary>
    [TemplatePart(Name = "PART_ItemsHolder", Type = typeof(Panel))]
    public class TabControlEx : TabControl
    {
        private Panel _itemsHolder;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _itemsHolder = GetTemplateChild("PART_ItemsHolder") as Panel;
            UpdateSelectedItem();
        }

        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsChanged(e);

            if (_itemsHolder == null)
                return;

            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                _itemsHolder.Children.Clear();
            }
            else if (e.OldItems != null)
            {
                foreach (object item in e.OldItems)
                {
                    ContentPresenter presenter = PresenterFor(item);
                    if (presenter != null)
                        _itemsHolder.Children.Remove(presenter);
                }
            }

            UpdateSelectedItem();
        }

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            base.OnSelectionChanged(e);
            UpdateSelectedItem();
        }

        private void UpdateSelectedItem()
        {
            if (_itemsHolder == null)
                return;

            object selected = SelectedItem;

            if (selected != null && PresenterFor(selected) == null)
            {
                _itemsHolder.Children.Add(new ContentPresenter
                {
                    Content = selected,
                    ContentTemplate = ContentTemplate,
                    ContentTemplateSelector = ContentTemplateSelector,
                    ContentStringFormat = ContentStringFormat,
                    Tag = selected
                });
            }

            foreach (UIElement child in _itemsHolder.Children)
            {
                ContentPresenter presenter = child as ContentPresenter;
                if (presenter == null)
                    continue;

                presenter.Visibility = ReferenceEquals(presenter.Tag, selected)
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
        }

        private ContentPresenter PresenterFor(object item)
        {
            foreach (UIElement child in _itemsHolder.Children)
            {
                ContentPresenter presenter = child as ContentPresenter;
                if (presenter != null && ReferenceEquals(presenter.Tag, item))
                    return presenter;
            }

            return null;
        }

        /// <summary>The presenter currently on screen, if any.</summary>
        internal ContentPresenter VisiblePresenter
        {
            get
            {
                if (_itemsHolder == null)
                    return null;

                foreach (UIElement child in _itemsHolder.Children)
                {
                    ContentPresenter presenter = child as ContentPresenter;
                    if (presenter != null && presenter.Visibility == Visibility.Visible)
                        return presenter;
                }

                return null;
            }
        }

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new TabControlExAutomationPeer(this);
        }

        /// <summary>
        /// The stock peer only publishes content it finds in a template part named
        /// PART_SelectedContentHost, which this control does not have - without this the
        /// whole content of every tab is invisible to assistive tech and to UI automation.
        /// </summary>
        private sealed class TabControlExAutomationPeer : TabControlAutomationPeer
        {
            public TabControlExAutomationPeer(TabControlEx owner)
                : base(owner)
            {
            }

            protected override List<AutomationPeer> GetChildrenCore()
            {
                List<AutomationPeer> children = base.GetChildrenCore() ?? new List<AutomationPeer>();

                ContentPresenter visible = ((TabControlEx)Owner).VisiblePresenter;
                if (visible != null)
                    AddPeers(visible, children);

                return children;
            }

            private static void AddPeers(DependencyObject parent, List<AutomationPeer> into)
            {
                int count = VisualTreeHelper.GetChildrenCount(parent);
                for (int i = 0; i < count; i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(parent, i);
                    UIElement element = child as UIElement;
                    AutomationPeer peer = element != null ? UIElementAutomationPeer.CreatePeerForElement(element) : null;

                    if (peer != null)
                        into.Add(peer);
                    else
                        AddPeers(child, into);   // keep digging past elements that have no peer
                }
            }
        }
    }
}
