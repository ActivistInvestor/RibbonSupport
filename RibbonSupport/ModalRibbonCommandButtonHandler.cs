
/// ModalRibbonCommandButtonHandler.cs
/// 
/// ActivistInvestor / Tony T
/// 
/// Distributed under the terms of the MIT license

using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.Ribbon;
using Autodesk.Windows.Extensions;

#pragma warning disable CS0612 // Type or member is obsolete

namespace Autodesk.AutoCAD.Ribbon.Extensions
{
   /// <summary>
   /// A concrete ICommand implementation that's designed
   /// to work with the RibbonCommandButton class. This
   /// class can be passed a RibbonCommandButton in its
   /// constructor, and it will use the CanExecuteManager
   /// to manage the RibbonCommandButton's enabled state
   /// so that the button is only enabled when there is
   /// a quiescent document in the drawing editor. 
   /// 
   /// Because a RibbonCommandButton usually provides the
   /// implementation of Execute(), an instance of this
   /// class will delegate to the Execute() method of the
   /// owning RibbonCommandButton. If an instance of this
   /// class is associated with many RibbonCommandButtons
   /// as described below, the Execute() method requires
   /// the parameter argument to be the RibbonCommandButton
   /// that is to be executed.
   /// 
   /// A single instance of this class can be used with
   /// many RibbonCommandButtons, by passing null to the
   /// constructor and assigning each RibbonCommandButtons's
   /// CommandParameter property to itself, and assigning 
   /// the handler to each button's CommandHandler property 
   /// as shown below:
   /// 
   ///   ModalRibbonCommandButtonHandler handler = 
   ///      new ModalRibbonCommandButtonHandler();
   ///
   ///   RibbonCommandButton button1 = new RibbonCommandButton("REGEN", "ID_REGEN");
   ///   button1.CommandParameter = button1;
   ///   button1.CommandHandler = handler;
   ///   
   ///   RibbonCommandButton button2 = new RibbonCommandButton("REGENALL", "ID_REGENALL");
   ///   button2.CommandParameter = button2;
   ///   button2.CommandHandler = handler;
   ///   
   /// When the instance is used by multiple RibbonCommandButtons,
   /// the Execute() method requires the parameter argument to be
   /// the instance of the RibbonCommandButton that is to execute,
   /// which is done by assigning each button's CommandParmeter
   /// property to the button as shown above.
   ///   
   /// If a ModalRibbonCommandButtonHandler is passed an 
   /// instance of a RibbonCommandButton to its constructor, 
   /// or has an instance of a RibbonCommandButton assigned
   /// to its Button property, the instance cannot be shared
   /// by multiple RibbonCommandButtons.
   /// </summary>

   public class ModalRibbonCommandButtonHandler : ModalCommandHandler
   {
      RibbonCommandButton button;

      public ModalRibbonCommandButtonHandler(RibbonCommandButton item = null)
      {
         this.Button = item;
         this.IsModal = true;
      }

      public RibbonCommandButton Button
      {
         get { return button; }
         set
         {
            if(button != value) 
            {
               if(button != null)
                  button.CommandHandler = null;
               button = value;
               if(button != null)
                  button.CommandHandler = this;
            }
         }
      }

      /// <summary>
      /// Can be used to simplify sharing of a single
      /// instance of this class with many instances of
      /// RibbonCommandButton, by simply calling the
      /// AddButtons() method and passing one or more 
      /// instances of RibbonCommandButton as arguments.
      /// </summary>

      public void AddButtons(params RibbonCommandButton[] buttons)
      {
         AddButtons((IEnumerable<RibbonCommandButton>)buttons);
      }

      public void AddButtons(IEnumerable<RibbonCommandButton> buttons)
      {
         if(buttons != null && buttons.Any())
         {
            foreach(RibbonCommandButton button in buttons)
            {
               if(button != null)
               {
                  button.CommandHandler = this;
                  button.CommandParameter = button;
               }
            }
         }
      }

      public override void Execute(object parameter)
      {
         if(button == null)
         {
            if(parameter is RibbonCommandButton target)
               target.Execute(this);
         }
         else
         {
            button.Execute(parameter);
         }
      }
   }

}