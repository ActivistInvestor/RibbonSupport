
/// RibbonEventManagerExample.cs
/// ActivistInvestor / Tony T
/// 
/// Distributed under the terms of the MIT license
/// 
/// An example showing the use of the RibbonEventManager 
/// class, and the ModalRibbonCommandButton class and 
/// supporting types.
/// 
/// In addition to demonstrating how to simply managing
/// ribbon content, this example also demonstrates the
/// use of components that allow ribbon UI elements to
/// synchronize their enabled state with the state of the
/// drawing editor. 
/// 
/// The buttons that are added to the RibbonTab in the
/// example below, will automatically enable and disable
/// themselves depending on whether there is an active,
/// quiescent document. 
/// 
/// If you try using the example, with the RibbonTab
/// added by it active and visible, try using various
/// AutoCAD commands, editing grips, and various other
/// operations, and you should see the example buttons
/// enable/disable automatically depending on what you
/// are doing in the editor. The example buttons will
/// only be enabled when there is an active document
/// with no active command.
/// 
/// For the curious, the core functionality for this is 
/// provided by the CanExecuteManager class.


using Ac2025Project.Test;
using Autodesk.AutoCAD.Ribbon;
using Autodesk.AutoCAD.Ribbon.Extensions;
using Autodesk.AutoCAD.Runtime;
using Autodesk.Windows;

/// TODO: If you use this example as a starting
/// point, then you should modify the argument 
/// to the ExtensionApplication attribute to be 
/// the name of the actual IExtensionApplication-
/// based class:

[assembly: ExtensionApplication(typeof(Namespace1.MyApplication))]

namespace Namespace1
{
   public class MyApplication : IExtensionApplication
   {
      /// Ribbon content should be assigned 
      /// to a static member variable:
      
      static RibbonTab myRibbonTab;

      /// <summary>
      /// IExtensionApplication.Initialize
      /// 
      /// Note: When using the RibbonEventManager,
      /// there is no need to defer execution of
      /// code until the Application.Idle event is
      /// raised, as the RibbonEventManager already
      /// does that for the programmer.
      /// 
      /// The handler for the InitializeRibbon event
      /// will not be called until the next Idle event 
      /// is raised.
      /// 
      /// </summary>

      public void Initialize()
      {
         /// Add a handler to the InitializeRibbon event.

         RibbonEventManager.InitializeRibbon += LoadMyRibbonContent;
      }

      /// <summary>
      /// Handler for the InitializeRibbon event.
      /// 
      /// This handler can be called multiple times,
      /// such as when a workspace is loaded. See the
      /// docs for RibbonEventManager for details on
      /// when/why this event handler will be called.
      /// </summary>

      private void LoadMyRibbonContent(object sender, RibbonStateEventArgs e)
      {
         /// Create the ribbon content if it has
         /// not already been created:

         CreateRibbonContent();

         /// Add the content to the ribbon:

         e.RibbonControl.Tabs.Add(myRibbonTab);
      }

      static void CreateRibbonContent()
      {
         if(myRibbonTab == null)
         {
            myRibbonTab = new RibbonTab();
            myRibbonTab.Id = "IDMyTab001";
            myRibbonTab.Name = "MyRibbonTab";
            myRibbonTab.Title = "MyRibbonTab";

            var src = new RibbonPanelSource();

            /// Add a ModalRibbonCommandButton
            var button = new ModalRibbonCommandButton("REGEN", "ID_REGENALL");
            button.Text = "  Test Button1  ";
            button.Size = RibbonItemSize.Large;
            button.ShowText = true;
            src.Items.Add(button);

            /// Add another ModalRibbonCommandButton
            button = new ModalRibbonCommandButton("REGENALL", "ID_REGENALL");
            button.Text = "  Test Button2  ";
            button.Size = RibbonItemSize.Large;
            button.ShowText = true;
            src.Items.Add(button);

            /// Add a standard RibbonCommandButton
            var button2 = new RibbonCommandButton("LINE", "ID_LINE");
            button2.Text = "  LINE  ";
            button2.Size = RibbonItemSize.Large;
            button2.ShowText = true;
            src.Items.Add(button2);

            /// Add aother standard RibbonCommandButton
            var button3 = new RibbonCommandButton("CIRCLE", "ID_CIRCLE");
            button3.Text = "  CIRCLE  ";
            button3.Size = RibbonItemSize.Large;
            button3.ShowText = true;
            src.Items.Add(button3);

            /// And one more...
            var button4 = new RibbonCommandButton("PLINE", "ID_PLINE");
            button4.Text = "  PLINE  ";
            button4.Size = RibbonItemSize.Large;
            button4.ShowText = true;
            src.Items.Add(button4);


            /// Create a ModalRibbonCommandButtonHandler 
            /// to act as the CommandHandler for all of 
            /// the RibbonCommandButtons:

            var handler = new ModalRibbonCommandButtonHandler();

            /// Set the above handler to be the CommandHandler 
            /// for the three RibbonCommandButtons:
            
            handler.SetAsHandler(button2, button3, button4);

            RibbonPanel panel = new RibbonPanel();
            panel.Source = src;
            myRibbonTab.Panels.Add(panel);
         }
      }

      public void Terminate()
      {
      }
   }

}