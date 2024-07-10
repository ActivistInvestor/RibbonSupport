
using Autodesk.AutoCAD.Runtime.Diagnostics;

/// Cached.cs  
/// 
/// ActivistInvestor / Tony T.
/// 
/// Distributed under the terms of the MIT license.

namespace System.Extensions
{
   /// <summary>
   /// Automates storage and updating of a value,
   /// using a caller-supplied factory method that 
   /// computes/updates the value as-needed. 
   /// 
   /// This class works like the framework's Lazy<T>,
   /// with the addition of offering the ability to
   /// invalidate the computed value, forcing it to
   /// be recomputed when subsequently requested.
   /// 
   /// The computed value is returned by the Value 
   /// property until the Invalidate() method is 
   /// called. 
   /// 
   /// Each time Invalidate() is called, the supplied 
   /// method will be called to recompute and update 
   /// the value the next time it is requested.
   /// </summary>
   /// <typeparam name="T">The type of the cached value</typeparam>
   
   public struct Cached<T>
   {
      bool valid = false;
      Func<T> factory;
      T value;

      public Cached(Func<T> factory)
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

      public static implicit operator T(Cached<T> value) => value.Value;
      public static implicit operator Cached<T>(Func<T> factory)
      {
         Assert.IsNotNull (factory, nameof(factory));
         return new Cached<T>(factory);
      }
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
         set
         {
            parameter = value;
            Invalidate();
         }
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


