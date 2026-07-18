using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SeqxcToolset.Tasks.PatternNumberTask
{
    public partial class ResolveMatchWindow : Window
    {
        public class Candidate
        {
            public string Name { get; set; }
            public string Label { get; set; }
            public bool IsStep { get; set; }
            public int Score { get; set; }
        }

        public string SelectedName { get; private set; }
        public bool Ignored { get; private set; }
        public bool IgnoreAllRemaining { get; private set; }

        private readonly List<Candidate> _all;

        public ResolveMatchWindow(IEnumerable<Candidate> candidates, string contextMessage)
        {
            InitializeComponent();
            MessageText.Text = contextMessage;
            _all = candidates.ToList();
            CandidateList.ItemsSource = _all;
            SelectTopItem();
            Loaded += (s, e) => FilterBox.Focus();
        }

        private void SelectTopItem()
        {
            if (CandidateList.Items.Count > 0)
            {
                CandidateList.SelectedIndex = 0;
                CandidateList.ScrollIntoView(CandidateList.SelectedItem);
            }
        }

        private void FilterBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string f = FilterBox.Text ?? "";
            CandidateList.ItemsSource = string.IsNullOrWhiteSpace(f)
                ? _all
                : _all.Where(c => c.Label.IndexOf(f, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
            SelectTopItem();
        }

        private void FilterBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) { TryApply(); e.Handled = true; }
            else if (e.Key == Key.Escape) { Ignore_Click(sender, e); e.Handled = true; }
            else if (e.Key == Key.Down)
            {
                CandidateList.Focus();
                if (CandidateList.Items.Count > 0)
                    CandidateList.SelectedIndex = Math.Min(CandidateList.SelectedIndex + 1, CandidateList.Items.Count - 1);
                e.Handled = true;
            }
        }

        private void Apply_Click(object sender, RoutedEventArgs e) => TryApply();

        private void CandidateList_MouseDoubleClick(object sender, MouseButtonEventArgs e) => TryApply();

        private void TryApply()
        {
            if (CandidateList.SelectedItem is Candidate c)
            {
                SelectedName = c.Name;
                DialogResult = true;
            }
        }

        private void Ignore_Click(object sender, RoutedEventArgs e)
        {
            Ignored = true;
            DialogResult = true;
        }

        private void IgnoreAll_Click(object sender, RoutedEventArgs e)
        {
            Ignored = true;
            IgnoreAllRemaining = true;
            DialogResult = true;
        }
    }
}
