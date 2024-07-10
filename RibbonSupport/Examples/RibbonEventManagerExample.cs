
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
using System.Linq;
using System.Windows.Controls;

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

            /// Add a ModalRibbonCommandButton.
            /// This button comes with its own CommandHandler:
            var button = new ModalRibbonCommandButton("REGEN", "ID_REGENALL");
            src.Items.Add(button);
            button.Text = "  REGENALL  ";
            button.Size = RibbonItemSize.Large;
            button.Orientation = Orientation.Vertical;
            button.ShowText = true;

            /// Add another ModalRibbonCommandButton:
            button = new ModalRibbonCommandButton("REGENALL", "ID_REGENALL");
            src.Items.Add(button);
            button.Text = "  REGEN  ";
            button.Size = RibbonItemSize.Large;
            button.Orientation = Orientation.Vertical;
            button.ShowText = true;

            /// Using best practices:
            /// 
            /// To avoid a lot of repetitive code, we define a
            /// simple specialization of RibbonCommandButton
            /// below (MyRibbonCommandButton), that sets common 
            /// property values in its constructor, avoiding the 
            /// need to do that for each instance created. The
            /// only parameter needed is the macro that executes
            /// when the user clicks the button.

            /// Add a few MyRibbonCommandButtons:
            src.Items.Add(new MyRibbonCommandButton("LINE"));
            src.Items.Add(new MyRibbonCommandButton("CIRCLE"));
            src.Items.Add(new MyRibbonCommandButton("SPLINE"));
            src.Items.Add(new MyRibbonCommandButton("PLINE"));

            /// The MyRibbonCommandButtons created above are not
            /// functional because they have no CommandHandler, so
            /// we'll create a single command handler and assign it
            /// to all of the buttons, using an extension method 
            /// included in this library:

            var handler = new ModalRibbonCommandButtonHandler();
            src.Items.SetDefaultCommandButtonHandler(handler);

            /// With the CommandHandlers set to an instance of the
            /// ModalRibbonCommandButtonHandler, all buttons will
            /// automatically disable/enable themselves when there
            /// is a command or other operation in progress.

            RibbonPanel panel = new RibbonPanel();
            panel.Source = src;
            myRibbonTab.Panels.Add(panel);
         }
      }

      public void Terminate()
      {
      }
   }

   /// <summary>
   /// A locally-used specialization of RibbonCommandButton 
   /// that merely presets some properties to desired common
   /// values for this specific use case.
   /// 
   /// If one needs to create many instances that should all
   /// have some of their properties set to common values, it
   /// is easier to just derive a specialization, and in the
   /// constructor, set the common property values.
   /// </summary>   
   
   public class MyRibbonCommandButton : RibbonCommandButton
   {
      public MyRibbonCommandButton(string macro)
         : base(macro, $"ID_{macro}")
      {
         this.Orientation = Orientation.Vertical;
         this.Size = RibbonItemSize.Large;
         this.ShowText = true;
         this.Text = $"  {macro}  ";
      }
   }

}