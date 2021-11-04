using System;
using System.Linq;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace WebForms.Binding
{
    /// <summary>
    /// A class used to generate names for HTML inputs that work with .NET framework model binding.
    /// </summary>
    public struct Input
    {
        StringBuilder name;

        /// <summary>
        /// Conditionally bind the 'checked' property.
        /// </summary>
        /// <param name="condition"></param>
        /// <returns></returns>
        public static string Checked(bool condition) =>
            condition ? "checked" : "";

        /// <summary>
        /// Conditionally bind the 'selected' property.
        /// </summary>
        /// <param name="condition"></param>
        /// <returns></returns>
        public static string Selected(bool condition) =>
            condition ? "selected" : "";

        /// <summary>
        /// Generate an input name that works with standard model binding.
        /// </summary>
        /// <typeparam name="T">The type of object being bound.</typeparam>
        /// <param name="obj">The object to which the input is being bound.</param>
        /// <param name="prop">The property name.</param>
        /// <returns></returns>
        public static Input Name => new Input { name = new System.Text.StringBuilder() };

        /// <summary>
        /// Add an indexer to the input name.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        /// <remarks>
        /// This is the fast, low-level interface for generating input names. You can use this overload to
        /// bind to collections by index. The following:
        /// <code>
        /// &lt;asp:Repeater runat="server" ...&gt;
        /// &lt;ItemTemplate&gt;
        ///     &lt;input name="&lt;%# InputName.Bind(myObj, nameof(myObj.SomeProperty))[Container.ItemIndex][nameof(myObj.SomeProperty[Container.ItemIndex].OtherProperty)] %&gt; ... />
        /// &lt;/asp:ItemTemplate&gt;
        /// &lt;/asp:Repeater&gt;
        /// </code>
        /// will generate a tag in web forms like so for 3 inputs:
        /// <code>
        ///     &lt;input name="SomeProperty[0].SomeProperty" ... />
        ///     &lt;input name="SomeProperty[2].SomeProperty" ... />
        ///     &lt;input name="SomeProperty[3].SomeProperty" ... />
        /// </code>
        /// which will bind to the properties at the specified indices.
        /// </remarks>
        public Input this[int i] =>
            new Input { name = name.Append('[').Append(i).Append(']') };

        /// <summary>
        /// Add a property to the input name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <remarks>
        /// This is the fast, low-level interface for generating input names. The following:
        /// <code>
        /// &lt;input name="&lt;%# InputName.Bind(myObj, nameof(myObj.SomeProperty))[nameof(myObj.SomeProperty.OtherProperty)] %&gt; ... />
        /// </code>
        /// will generate a tag in web forms like so:
        /// <code>
        /// &lt;input name="SomeProperty.OtherProperty" ... />
        /// </code>
        /// </remarks>
        public Input this[string name] =>
            new Input { name = this.name.Length == 0 ? this.name.Append(name) : this.name.Append('.').Append(name) };

        /// <summary>
        /// Use expressions to specify the property being bound.
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
        /// </remarks>
        public static Input Bind<T0, T1>(T0 obj, Expression<Func<T0, T1>> prop) =>
            new Input { name = new System.Text.StringBuilder() }.Bind(prop.Body);

        Input Bind(Expression e)
        {
            switch (e)
            {
                case MemberExpression m:
                    return Bind(m.Expression)[m.Member.Name];
                case IndexExpression i:
                    return Bind(i.Object)[(int)Value(i.Arguments.Single())];
                case MethodCallExpression c when c.Method.Name == "get_Item":
                    return Bind(c.Object)[(int)Value(c.Arguments.Single())];
                case ParameterExpression p:
                    return this; // bottom out at parameter
                default: throw new NotSupportedException($"Unrecognized expression type ({e.NodeType}): {e}");
            }
        }
        object Value(Expression e)
        {
            switch (e)
            {
                case MemberExpression m when m.Member is System.Reflection.PropertyInfo p:
                    return p.GetValue(Value(m.Expression));
                case MemberExpression m when m.Member is System.Reflection.FieldInfo f:
                    return f.GetValue(Value(m.Expression));
                case ConstantExpression c:
                    return c.Value;
                default: throw new NotSupportedException($"Unrecognized expression type ({e.NodeType}): {e}");
            }
        }

        /// <summary>
        /// Generate an input name.
        /// </summary>
        /// <returns></returns>
        public override string ToString() => name.ToString();
    }
}
