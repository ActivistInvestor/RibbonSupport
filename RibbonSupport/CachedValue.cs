
using Autodesk.AutoCAD.Runtime.Diagnostics;

/// CachedValue.cs  
/// 
/// ActivistInvestor / Tony T.
/// 
/// Distributed under the terms of the MIT license.

namespace System.Extensions
{
   /// <summary>
   /// Automates storage of a value and a method
   /// that computes/updates the value as-needed. 
   /// 
   /// The stored value is returned by the Value 
   /// property until the Invalidate() method is 
   /// called. Each time Invalidate() is called, 
   /// the supplied method will be called again to 
   /// update the cached value the next time the
   /// Value property is referenced.
   /// </summary>
   /// <typeparam name="T">The type of the cached value</typeparam>
   
   public struct CachedValue<T>
   {
      bool valid = false;
      Func<T> factory;
      T value;

      public CachedValue(Func<T> factory)
      {
         Assert.IsNotNull(factory, nameof(factory));
         this.factory = factory;
      }

      public void Invalidate()
      {
         this.valid = false;
      }

      public T Value
      {
         get 
         { 
            if(!valid)
            {
               value = factory();
               valid = true;
            }
            return value; 
         }
      }

      public static implicit operator T(CachedValue<T> value) => value.Value;
   }

   /// <summary>
   /// A version of Cached that uses a parameter to compute the value.
   /// </summary>
   /// <typeparam name="T">The type of the cached value</typeparam>
   /// <typeparam name="TParameter">The type of the parameter used to compute the value</typeparam>
   
   public struct Cached<T, TParameter>
   {
      bool valid = false;
      Func<TParameter, T> factory;
      T value;
      TParameter parameter;

      public Cached(TParameter parameter, Func<TParameter, T> factory)
      {
         Assert.IsNotNull(factory, nameof(factory));
         this.parameter = parameter;
         this.factory = factory;
      }

      public void Invalidate()
      {
         this.valid = false;
      }

      public void Invalidate(TParameter parameter)
      {
         this.parameter = parameter;
         this.valid = false;
      }

      public TParameter Parameter
      { 
         get => parameter; 
         set => parameter = value; 
      }

      public T Value
      {
         get
         {
            if(!valid)
            {
               value = factory(parameter);
               valid = true;
            }
            return value;
         }
      }

      public static implicit operator T(Cached<T, TParameter> value) => value.Value;
   }

}


