using System;
using System.ComponentModel;
using System.Diagnostics;

namespace SequenceCheckCS.My
{
    internal static partial class MyProject
    {
        internal partial class MyForms
        {

            [EditorBrowsable(EditorBrowsableState.Never)]
            public CalRule m_CalRule;

            public CalRule CalRule
            {
                [DebuggerHidden]
                get
                {
                    m_CalRule = Create__Instance__(m_CalRule);
                    return m_CalRule;
                }
                [DebuggerHidden]
                set
                {
                    if (ReferenceEquals(value, m_CalRule))
                        return;
                    if (value is not null)
                        throw new ArgumentException("Property can only be set to Nothing");
                    Dispose__Instance__(ref m_CalRule);
                }
            }


            [EditorBrowsable(EditorBrowsableState.Never)]
            public MainForm m_MainForm;

            public MainForm MainForm
            {
                [DebuggerHidden]
                get
                {
                    m_MainForm = Create__Instance__(m_MainForm);
                    return m_MainForm;
                }
                [DebuggerHidden]
                set
                {
                    if (ReferenceEquals(value, m_MainForm))
                        return;
                    if (value is not null)
                        throw new ArgumentException("Property can only be set to Nothing");
                    Dispose__Instance__(ref m_MainForm);
                }
            }

        }


    }
}