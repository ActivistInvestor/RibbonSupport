
/// ModalRibbonCommandItemHandler.cs
/// 
/// ActivistInvestor / Tony T
/// 
/// Distributed under the terms of the MIT license


using Autodesk.Windows;
using Autodesk.Windows.Extensions;

#pragma warning disable CS0612 // Type or member is obsolete

namespace Autodesk.AutoCAD.Ribbon.Extensions
{
   /// <summary>
   /// A reusable ICommand that implments CanExecute()
   /// and allows a RibbonItem to only be enabled when 
   /// there is a quiescent document in the drawing editor.
   /// 
   /// This class is abstract and must be derived from.
   /// The Execute() method is the only method that must
   /// be implemented in derived concrete types.
   /// </summary>

   public abstract class ModalRibbonCommandItemHandler : ModalCommandHandler
   {
      RibbonCommandItem item;

      public ModalRibbonCommandItemHandler(RibbonCommandItem item) 
      {
         this.item = item;
         this.item.CommandHandler = this;
      }

   }

}