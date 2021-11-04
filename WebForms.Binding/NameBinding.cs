using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace WebForms.Binding
{
    public static class NameBinding
    {
        /// <summary>
        /// Generate a binding to the member given by the expression.
        /// </summary>
        /// <typeparam name="T0"></typeparam>
        /// <typeparam name="T1"></typeparam>
        /// <param name="obj"></param>
        /// <param name="prop"></param>
        /// <returns></returns>
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
        public static Input BindTo<T0, T1>(this T0 obj, Expression<Func<T0, T1>> prop) =>
            Input.Bind(obj, prop);
    }
}
