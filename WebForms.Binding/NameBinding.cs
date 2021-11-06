using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace WebForms.Binding
{
    /// <summary>
    /// Convenient static methods for data binding.
    /// </summary>
    public static class NameBinding
    {
        /// <summary>
        /// Generate a binding to the member given by the expression.
        /// </summary>
        /// <typeparam name="T0">The type of object being bound.</typeparam>
        /// <typeparam name="T1">The type of the property being bound.</typeparam>
        /// <param name="obj">The object whose properties are being data bound.</param>
        /// <param name="prop">The property being data bound.</param>
        /// <returns>An input name that binds to the designated property.</returns>
        /// <remarks>
        /// A high-level interface to name binding using expressions, similar to how MVC works:
        /// <code>
        /// &lt;asp:Repeater runat="server" ...&gt;
        /// &lt;ItemTemplate&gt;
        ///     &lt;input name="&lt;%# myObj.BindTo(x => x.SomeProperty[Container.ItemIndex].OtherProperty) %&gt; ... />
        /// &lt;/asp:ItemTemplate&gt;
        /// &lt;/asp:Repeater&gt;
        /// </code>
        /// will generate a tag in web forms like so for 3 inputs:
        /// <code>
        ///     &lt;input name="SomeProperty[0].SomeProperty" ... />
        ///     &lt;input name="SomeProperty[2].SomeProperty" ... />
        ///     &lt;input name="SomeProperty[3].SomeProperty" ... />
        /// </code>
        public static Html BindTo<T0, T1>(this T0 obj, Expression<Func<T0, T1>> prop) =>
            Html.Bind(obj, prop);

        /// <summary>
        /// Efficient method to return a simple property name for binding.
        /// </summary>
        /// <param name="propertyName">The property name being bound.</param>
        /// <returns>A string for a name that can be used for data binding.</returns>
        public static string BindTo<T>(this T obj, string propertyName) =>
            propertyName;
    }
}
