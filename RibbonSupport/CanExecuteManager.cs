/// CanExecuteManager.cs
/// 
/// ActivistInvestor / Tony T
/// 
/// Distributed under the terms of the MIT license

using System;
using System.Windows.Controls;
using System.Windows.Input;

namespace Autodesk.AutoCAD.ApplicationServices.Extensions
{
   /// <summary>
   /// Forces the WPF framework to query the CanExecute()
   /// method of all registered ICommands, to update their 
   /// UI state when AutoCAD commands start or end; when the 
   /// active document changes; and when the locked state of 
   /// the active document changes.
   /// 
   /// To enable updating one must simply do this:
   /// 
   ///    CanExecuteManager.Enabled = true;
   ///    
   /// The default implementation of CanExecute() for the
   /// RibbonCommandButton always returns true. 
   /// 
   /// A specialization of RibbonCommandButton included in
   /// this library (ModalRibbonCommandButton) provides the
   /// functionality needed to automatically enable/disable 
   /// itself depending on if there is a quiescent active 
   /// document.
   /// 
   /// An example of ModalRibbonCommandButton can be seen in the
   /// RibbonEventManagerExample.cs file.
   /// 
   /// </summary>

   public sealed class CanExecuteManager : IDisposable
   {
      static DocumentCollection docs = Application.DocumentManager;
      static CanExecuteManager instance;
      private bool disposed;
      bool eventsEnabled = false;

      CanExecuteManager()
      {
         EnableEvents(true);
      }

      public static bool Enabled 
      { 
         get 
         {
            return instance != null;
         }
         set
         {
            if(instance != null ^ value)
            {
               if(value)
               {
                  instance = new CanExecuteManager();
               }
               else
               {
                  instance?.Dispose();
                  instance = null;
               }
            }
         }
      }

      public static bool HasQuiescentDocument
      {
         get
         {
            Document doc = docs.MdiActiveDocument;
            if(doc != null)
            {
               return doc.Editor.IsQuiescent
                  && !doc.Editor.IsDragging
                  && (doc.LockMode() & DocumentLockMode.NotLocked)
                        == DocumentLockMode.NotLocked;
            }
            return false;
         }
      }

      void EnableEvents(bool enable)
      {
         if((enable ^ eventsEnabled) && !isQuitting)
         {
            eventsEnabled = enable;
            if(enable)
            {
               docs.DocumentLockModeChanged += documentLockModeChanged;
               docs.DocumentActivated += documentActivated;
               docs.DocumentDestroyed += documentDestroyed;
               Application.QuitWillStart += quit;
            }
            else
            {
               docs.DocumentLockModeChanged -= documentLockModeChanged;
               docs.DocumentActivated -= documentActivated;
               docs.DocumentDestroyed -= documentDestroyed;
               Application.QuitWillStart -= quit;
            }
         }
      }

      void InvalidateRequerySuggested()
      {
         CommandManager.InvalidateRequerySuggested();
      }

      /// <summary>
      /// Handlers of driving events:
      /// </summary>

      void documentLockModeChanged(object sender, DocumentLockModeChangedEventArgs e)
      {
         if(e.Document == docs.MdiActiveDocument && !e.GlobalCommandName.ToUpper().Contains("ACAD_DYNDIM"))
            InvalidateRequerySuggested();
      }

      void documentActivated(object sender, DocumentCollectionEventArgs e)
      {
         InvalidateRequerySuggested();
      }

      void documentDestroyed(object sender, DocumentDestroyedEventArgs e)
      {
         InvalidateRequerySuggested();
      }

      void quit(object sender, EventArgs e)
      {
         try
         {
            EnableEvents(false);
         }
         catch
         {
         }
         finally
         {
            isQuitting = true;
         }
      }

      bool isQuitting = false;

      public bool IsQuitting => isQuitting;

      public void Dispose()
      {
         if(!disposed)
         {
            this.disposed = true;
            if(!isQuitting)
            {
               EnableEvents(false);
            }
         }
         GC.SuppressFinalize(this);
      }
   }
}


