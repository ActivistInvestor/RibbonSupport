﻿/// RibbonEventManager.cs
/// 
/// ActivistInvestor / Tony T
/// 
/// Distributed under the terms of the MIT license
/// 

using System;
using System.Collections.Generic;
using System.Extensions;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Internal;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Runtime.Diagnostics;
using Autodesk.Windows;

namespace Autodesk.AutoCAD.Ribbon.Extensions
{
   /// Simplified ribbon content management:
   /// 
   /// A class that provides a simplified means of 
   /// initializing and managing application-provided
   /// content for AutoCAD's ribbon.


   /// <summary>
   /// RibbonEventManager exposes a single event that can be
   /// handled to be notified whenever it is necessary to add 
   /// or refresh ribbon content.
   /// 
   /// The InitializeRibbon event:
   /// 
   /// The typical usage pattern for using this event, is to
   /// simply add a handler to it when the application/extension
   /// is loaded (e.g., from an IExtensionApplication.Initialize
   /// method). If that is done, it isn't necessary to check to
   /// see if the ribbon exists, add handlers to other events, etc.. 
   /// One need only add a handler to the RibbonEventManager's 
   /// InitializeRibbon event, and in the handler, add content to 
   /// the ribbon.
   /// 
   /// Using this class and its single event relieves the developer
   /// from the complicated burden of having to check conditions and
   /// handle multiple events to ensure that their content is always 
   /// present on the ribbon.
   /// 
   /// A minimal example IExtensionApplication that uses this class
   /// to manage ribbon content:
   /// 
   /// <code>
   ///  
   ///   public class MyApplication : IExtensionApplication
   ///   {
   ///      public void Initialize()
   ///      {
   ///         RibbonEventManager.InitializeRibbon += LoadMyRibbonContent;
   ///      }
   ///      
   ///      private void LoadMyRibbonContent(object sender, RibbonStateEventArgs e)
   ///      {
   ///         // Here, one can safely assume the ribbon exists,
   ///         // and that content should be added to it.
   ///      }
   ///
   ///      public void Terminate()
   ///      {
   ///      }
   ///   }
   /// 
   /// </code>
   /// 
   /// The handler for the InitializeRibbon event will be 
   /// called whenever it is necessary to add content to 
   /// the ribbon, which includes:
   ///   
   ///   1. When the handler is first added to the 
   ///      InitializeRibbon event and the ribbon 
   ///      currently exists.
   ///   
   ///   2. When the ribbon is first created and shown 
   ///      when it did not exist when the handler was 
   ///      added to the InitializeRibbon event.
   ///      
   ///   3. When a workspace is loaded, after having 
   ///      added content to the ribbon.
   ///   
   /// The State property of the event argument indicates
   /// which of the these three conditions triggered the
   /// event.
   /// 
   /// 6/4/24 Revisons:
   /// 
   /// 1. The IdleAction class has been replaced with the
   ///    IdleAwaiter class, to defer execution of code 
   ///    until the next Application.Idle event is raised.
   /// 
   /// 2. A new AddRibbonTabs() method was added to the event
   ///    argument type (RibbonStateEventArgs), that will add
   ///    one or more ribbon tabs to the ribbon if they are not
   ///    already present on the ribbon.
   ///    
   /// 7/7/24
   /// 
   /// 1. Revision 1 above has been rolled-back due to issues
   ///    related to unhandled exceptions thrown from await'ed 
   ///    continuations, that cause AutoCAD to terminate.
   ///    
   /// 7/9/24 
   /// 
   /// 1. Merging CanExecuteManager into RibbonEventManager.
   /// 
   /// Test scenarios covered:
   /// 
   /// 1. Client extension application loaded at startup:
   /// 
   ///    - With ribbon existing at startup.
   ///    
   ///    - With ribbon not existing at startup,
   ///      and subsequently created by issuing 
   ///      the RIBBON command.
   ///       
   /// 2. Client extension application loaded at any point
   ///    during session via NETLOAD or demand-loading when 
   ///    a registered command is first issued:
   ///    
   ///    - With ribbon existing at load-time.
   ///    
   ///    - With ribbon not existing at load-time, 
   ///      and subsequently created by issuing the 
   ///      RIBBON command.
   /// 
   /// 3. With client extension loaded and ribbon content
   ///    already added to an existing ribbon, that is
   ///    subsequently removed by one of these actions:
   ///    
   ///    - CUI command
   ///    - MENULOAD command.
   ///    - CUILOAD/CUIUNLOAD commands.
   ///    
   /// In all of the above cases, the InitializeRibbon 
   /// event is raised to signal that content should be
   /// added to the ribbon.
   /// 
   /// To summarize, if your app adds content to the ribbon
   /// and you want to ensure that it is always added when
   /// needed, just handle the InitializeRibbon event, and 
   /// add the content to the ribbon in the event's handler.
   ///    
   /// Feel free to post comments in the repo discussion
   /// regarding other scenarious not covered, or about
   /// any other issues or bugs you may have come across.
   /// 
   /// </summary>

   public static class RibbonEventManager
   {
      static DocumentCollection documents = Application.DocumentManager;
      static event RibbonStateEventHandler initializeRibbon = null;
      static bool initialized = false;
      static Cached<bool> quiescent = new Cached<bool>(GetIsQuiescent);

      static RibbonEventManager()
      {
         if(RibbonCreated)
            Initialize(RibbonState.Active);
         else
            RibbonServices.RibbonPaletteSetCreated += ribbonPaletteSetCreated;
      }

      static void Initialize(RibbonState state)
      {
         Idle.Invoke(delegate ()
         {
            if(initializeRibbon != null)
            {
               try
               {
                  initializeRibbon?.Invoke(RibbonPaletteSet, new RibbonStateEventArgs(state));
               }
               catch(System.Exception ex)
               {
                  UnhandledExceptionFilter.CerOrShowExceptionDialog(ex);
               }
            }
            RibbonPaletteSet.WorkspaceLoaded += workspaceLoaded;
            initialized = true;
         });
      }
      
      static void RaiseInitializeRibbon(RibbonState state)
      {
         if(initializeRibbon != null)
         {
            Idle.Invoke(delegate ()
            {
               try
               {
                  initializeRibbon?.Invoke(RibbonPaletteSet, new RibbonStateEventArgs(state));
               }
               catch(System.Exception ex)
               {
                  UnhandledExceptionFilter.CerOrShowExceptionDialog(ex);
               }
            });
         }
      }

      private static void ribbonPaletteSetCreated(object sender, EventArgs e)
      {
         RibbonServices.RibbonPaletteSetCreated -= ribbonPaletteSetCreated;
         Initialize(RibbonState.Initalizing);
      }

      private static void workspaceLoaded(object sender, EventArgs e)
      {
         if(RibbonControl != null)
            RaiseInitializeRibbon(RibbonState.WorkspaceLoaded);
      }

      /// <summary>
      /// If a handler is added to this event and the ribbon 
      /// exists, the handler will be invoked immediately, or
      /// on the next Idle event, depending on the execution
      /// context the handler is added from.
      /// 
      /// Note: Adding the same event handler to this event
      /// multiple times will result in undefined behavior.
      /// </summary>

      public static event RibbonStateEventHandler InitializeRibbon
      {
         add
         {
            if(value == null)
               throw new ArgumentNullException(nameof(value));
            if(initialized)
               InvokeHandler(value);
            else
               initializeRibbon += value;
         }
         remove
         {
            initializeRibbon -= value;
         }
      }

      static void InvokeHandler(RibbonStateEventHandler handler)
      {
         Idle.Invoke(delegate ()
         {
            try
            {
               handler(RibbonPaletteSet, new RibbonStateEventArgs(RibbonState.Active));
            }
            catch(System.Exception ex)
            {
               UnhandledExceptionFilter.CerOrShowExceptionDialog(ex);
               return;
            }
            initializeRibbon += handler;
         });
      }

      /// <summary>
      /// Forces the WPF framework to requery the CanExecute()
      /// method of all registered ICommands, to update their 
      /// UI state when:
      /// 
      ///   1. AutoCAD or LISP commands start and end.
      ///   2. The active document changes.
      ///   3. The lock state of the active document changes.
      /// 
      /// To enable updating one must simply do this:
      /// 
      ///    RibbonEventManager.QueryCanExecuteEnabled = true;
      ///    
      /// The default implementation of CanExecute() for the
      /// RibbonCommandButton always returns true. 
      /// 
      /// A specialization of RibbonCommandButton included in
      /// this library (ModalRibbonCommandButton) provides the
      /// functionality needed to automatically enable/disable 
      /// itself depending on if there is a quiescent active 
      /// document, using the functionality provided by this
      /// class.
      /// 
      /// An example ModalRibbonCommandButton can be seen in 
      /// the RibbonEventManagerExample.cs file.
      /// 
      /// </summary>

      public static bool QueryCanExecuteEnabled
      {
         get => EditorStateObserver.Enabled;
         set => EditorStateObserver.Enabled = value;
      }

      /// <summary>
      /// This can be called at a high frequency by numerous
      /// ICommands, which can be very expensive. To minimize
      /// the overhead of referencing this property, the value
      /// it returns is cached and reused until one of the events 
      /// that signals that the state may have changed is raised.
      /// 
      /// Returns a value indicating if there is an active
      /// document, and it is in a quiescent state. If there
      /// are no documents open, this property returns false.
      /// </summary>

      public static bool IsQuiescentDocument => quiescent.Value;

      static bool GetIsQuiescent()
      {
         Document doc = documents.MdiActiveDocument;
         if(doc != null)
         {
            return doc.Editor.IsQuiescent
               && !doc.Editor.IsDragging
               && (doc.LockMode() & DocumentLockMode.NotLocked)
                     == DocumentLockMode.NotLocked;
         }
         return false;
      }

      static void InvalidateQuiescentState()
      {
         quiescent.Invalidate();
         OnIsQuiescentDocumentChanged(EventArgs.Empty);
         CommandManager.InvalidateRequerySuggested();
      }

      public static event EventHandler IsQuiescentDocumentChanged;

      static void OnIsQuiescentDocumentChanged(EventArgs e)
      {
         IsQuiescentDocumentChanged?.Invoke(null, e);
      }

      public static bool IsQuiescent => Utils.IsInQuiescentState();
      
      public static bool RibbonCreated => RibbonControl != null;

      public static RibbonPaletteSet RibbonPaletteSet =>
         RibbonServices.RibbonPaletteSet;

      public static RibbonControl? RibbonControl =>
         RibbonPaletteSet?.RibbonControl;

      /// Helper classes
      /// 
      /// <summary>
      /// Delays execution of a supplied action 
      /// until the next Idle event is raised.
      /// </summary>

      class Idle
      {
         Action action;
         static Idle current = null;
         Idle(Action action)
         {
            this.action = action;
            Application.Idle += idle;
         }

         /// <summary>
         /// If this method is called from an action that
         /// was passed to a previous call to this method,
         /// and the deferred argument is false, the action 
         /// executes immediately and is not deferred until 
         /// the next idle event.
         /// </summary>

         public static void Invoke(Action action, bool deferred = false)
         {
            Assert.IsNotNull(action, nameof(action));
            if(current != null && !deferred)
               action();
            else
               new Idle(action);
         }

         private void idle(object sender, EventArgs e)
         {
            Application.Idle -= idle;
            if(action != null)
            {
               current = this;
               try
               {
                  action();
                  action = null;
               }
               finally
               {
                  current = null;
               }
            }
         }
       
         public class Distinct
         {
            static HashSet<Action> actions = new HashSet<Action>();

            /// <summary>
            /// If the specified Action has already been passed
            /// to this method but has not yet executed, it is
            /// ignored and the action will only execute once on
            /// the next idle event.
            /// 
            /// Hence, this method can be called multiple times
            /// with the same action argument, but that action
            /// will execute only once on the next idle event.
            /// </summary>
            /// <param name="action"></param>
            /// <returns>True if execution of the given action is
            /// not already pending, or false if it is.</returns>

            public static bool Invoke(Action action)
            {
               Assert.IsNotNull(action, nameof(action));
               if(actions.Add(action))
               {
                  new Wrapper(action);
                  return true;
               }
               return false;
            }

            class Wrapper
            {
               Action action;

               public Wrapper(Action action)
               {
                  this.action = action;
                  Idle.Invoke(Invoke);
               }
               public void Invoke()
               {
                  if(action != null)
                  {
                     actions.Remove(action);
                     action();
                     action = null;
                  }
               }
            }
         }
      }

      class EditorStateObserver : IDisposable
      {
         static DocumentCollection docs = Application.DocumentManager;
         static EditorStateObserver instance;
         private bool disposed;
         bool enabled = false;

         EditorStateObserver()
         {
            EnableEvents(true);
            RibbonEventManager.InvalidateQuiescentState();
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
                     instance = new EditorStateObserver();
                  }
                  else
                  {
                     instance?.Dispose();
                     instance = null;
                  }
               }
            }
         }

         void EnableEvents(bool value)
         {
            if((value ^ enabled) && !isQuitting)
            {
               enabled = value;
               if(value)
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
            RibbonEventManager.InvalidateQuiescentState();
         }

         /// <summary>
         /// Handlers of driving events:
         /// 
         /// These events signal that the effective-quiescent state
         /// may have changed. When one of them is raised, WPF is told
         /// to requery the CanExecute() method of registered ICommands,
         /// and the state of connected UI elements is updated.
         /// 
         /// It should be noted that this can become very expensive, and
         /// is probably why AutoCAD doesn't 'kick' WPF into doing it.
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

   /// <summary>
   /// Indicates the context in which the 
   /// InitializeRibbon event is raised.
   /// </summary>

   public enum RibbonState
   {
      /// <summary>
      /// The ribbon exists but was not 
      /// previously-initialized.
      /// </summary>
      Active = 0,

      /// <summary>
      /// The ribbon was just created.
      /// </summary>
      Initalizing = 1,

      /// <summary>
      /// The ribbon exists and was previously
      /// initialized, and a workspace was just
      /// loaded, requiring application-provided 
      /// ribbon content to be added again.
      /// </summary>
      WorkspaceLoaded = 2,

      /// <summary>
      /// Indicates that ribbon content should be
      /// reloaded for unspecified reasons.
      /// </summary>
      RefreshContent = 3
   }

   public delegate void RibbonStateEventHandler(object sender, RibbonStateEventArgs e);

   public class RibbonStateEventArgs : EventArgs
   {
      public RibbonStateEventArgs(RibbonState state)
      {
         this.State = state;
      }

      /// <summary>
      /// Conditionally adds one or more tabs to the Ribbon
      /// if they are not already present on it.
      /// </summary>
      /// <param name="items">One or more RibbonTab instances</param>
      /// <returns>The number of tabs added to the ribbon</returns>

      public int AddRibbonTabs(params RibbonTab[] items)
      {
         return AddRibbonTabs((IEnumerable<RibbonTab>)items);
      }

      public int AddRibbonTabs(IEnumerable<RibbonTab> items)
      {
         if(RibbonControl == null)
            return 0;
         if(items == null)
            throw new ArgumentNullException(nameof(items));
         return RibbonControl.TryAddTabs(items.ToArray());
      }

      public RibbonState State { get; private set; }
      public RibbonPaletteSet RibbonPaletteSet =>
         RibbonServices.RibbonPaletteSet;
      public RibbonControl RibbonControl =>
         RibbonPaletteSet?.RibbonControl;
   }
}

