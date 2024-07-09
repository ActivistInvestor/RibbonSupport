
/// ModalRibbonCommandButton.cs
/// 
/// ActivistInvestor / Tony T
/// 
/// Distributed under the terms of the MIT license

#pragma warning disable CS0612 // Type or member is obsolete

namespace Autodesk.AutoCAD.Ribbon.Extensions
{
   /// <summary>
   /// A specialization of RibbonCommandButton that is only enabled
   /// when there is an active, quiescent document
   /// </summary>

   public class ModalRibbonCommandButton : RibbonCommandButton
   {
      ModalRibbonCommandButtonHandler commandHandler;
      public ModalRibbonCommandButton()
      {
         this.CommandHandler = new ModalRibbonCommandButtonHandler(this);
      }

      public ModalRibbonCommandButton(string sMenuMacro, string sMenuMacroId)
         : base(sMenuMacro, sMenuMacroId) 
      {
         this.CommandHandler = new ModalRibbonCommandButtonHandler(this);
      }

      public ModalRibbonCommandButton(object value)
         : base(value)
      {
         this.CommandHandler = new ModalRibbonCommandButtonHandler(this);
      }
   }

}