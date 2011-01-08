// A command filter for the editor.  Command filters get an opportunity to observe and handle commands before and after the editor acts on them.

using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;

//Based on the template at https://github.com/NoahRic/EditorItemTemplates/blob/master/CommandFilterTemplate.cs
//Originally referenced at http://blogs.msdn.com/b/visualstudio/archive/2009/11/23/what-s-new-for-editor-extenders-in-beta-2.aspx

namespace EditorRefactoringMonitor
{
    [Export(typeof (IVsTextViewCreationListener))]
    [ContentType("code")]
    [TextViewRole(PredefinedTextViewRoles.Interactive)]
    internal class VsTextViewCreationListener : IVsTextViewCreationListener
    {
        #region IVsTextViewCreationListener Members

        public void VsTextViewCreated(IVsTextView textViewAdapter)
        {
            var filter = new CommandFilter();

            IOleCommandTarget next;
            if (ErrorHandler.Succeeded(textViewAdapter.AddCommandFilter(filter, out next)))
                filter.Next = next;
        }

        #endregion
    }

    internal class CommandFilter : IOleCommandTarget
    {
        /// <summary>
        /// The next command target in the filter chain (provided by <see cref="IVsTextView.AddCommandFilter"/>).
        /// </summary>
        internal IOleCommandTarget Next { get; set; }

        #region IOleCommandTarget Members

        public int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            //http://msdn.microsoft.com/en-us/library/cc826118.aspx lists some ID's
            //http://msdn.microsoft.com/en-us/library/cc826040.aspx tells us where to look for definitions of menus and commands.

            //Visual Studio SDK installation path\VisualStudioIntegration\Common\Inc\SharedCmdDef.vsct contained references to ECMD_RENAME and contained the following: <Extern href="stdidcmd.h"/>
            //Visual Studio SDK installation path\VisualStudioIntegration\Common\Inc\stdidcmd.h contained "#define ECMD_RENAME 1550".
            const uint ecdRename = 1550;
            if (nCmdID == ecdRename)
            {
                Debug.WriteLine("RENAME was invoked");
            }
            // TODO: Command handling before passing commands to the Next command target

            // Pass the command on to our next command target (if we want it the editor to handle it)
            int hresult = Next.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);

            // TODO: Command handling after passing commands to the Next command target.

            return hresult;
        }

        public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            // TODO: If we want to block or enable commands that the editor handles by default, do it before passing commands to Next

            return Next.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);
        }

        #endregion

        #region Private helpers

        /// <summary>
        /// Get the char for a <see cref="VSConstants.VSStd2KCmdID.TYPECHAR"/> command.
        /// </summary>
        /// <param name="pvaIn">The "pvaIn" arg passed to <see cref="Exec"/>.</param>
        private char GetTypedChar(IntPtr pvaIn)
        {
            return (char) (ushort) Marshal.GetObjectForNativeVariant(pvaIn);
        }

        #endregion
    }
}