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
    public struct Html
    {
        StringBuilder name;

        /// <summary>
        /// Conditionally bind the 'checked' attribute.
        /// </summary>
        /// <param name="condition">The condition to check.</param>
        /// <returns>The 'checked' attribute is rendered if the condition is true.</returns>
        public static string Checked(bool condition) =>
            Attribute(condition, "checked");

        /// <summary>
        /// Conditionally bind the 'selected' attribute.
        /// </summary>
        /// <param name="condition">The condition to check.</param>
        /// <returns>The 'selected' attribute is rendered if the condition is true.</returns>
        public static string Selected(bool condition) =>
            Attribute(condition, "selected");

        /// <summary>
        /// Conditionally bind the 'readonly' attribute.
        /// </summary>
        /// <param name="condition">The condition to check.</param>
        /// <returns>The 'readonly' attribute is rendered if the condition is true.</returns>
        public static string ReadOnly(bool condition) =>
            Attribute(condition, "readonly");

        /// <summary>
        /// Conditionally bind an attribute.
        /// </summary>
        /// <param name="condition">The condition to check.</param>
        /// <param name="attribute">The attribute to set.</param>
        /// <returns>The given attribute is rendered if the condition is true.</returns>
        public static string Attribute(bool condition, string attribute) =>
            Render(condition, attribute, "");

        /// <summary>
        /// Render content according to a condition.
        /// </summary>
        /// <param name="condition">The condition to check.</param>
        /// <param name="ifTrue">The content to render if the condition is true.</param>
        /// <param name="ifFalse">The content to render if the condition is false.</param>
        /// <returns>The content to render.</returns>
        public static string Render(bool condition, string ifTrue, string ifFalse) =>
            condition ? ifTrue : ifFalse;

        /// <summary>
        /// Generate an input name that works with standard model binding.
        /// </summary>
        /// <typeparam name="T">The type of object being bound.</typeparam>
        /// <param name="obj">The object to which the input is being bound.</param>
        /// <param name="prop">The property name.</param>
        /// <returns>An input name that binds to the designated property.</returns>
        public static Html Name => new Html { name = new System.Text.StringBuilder() };

        /// <summary>
        /// Add an indexer to the input name.
        /// </summary>
        /// <param name="i">The index into the collection.</param>
        /// <returns>An input name that binds to the designated property.</returns>
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
        public Html this[int i] =>
            new Html { name = name.Append('[').Append(i).Append(']') };

        /// <summary>
        /// Add a property to the input name.
        /// </summary>
        /// <param name="name">The member name.</param>
        /// <returns>An input name that binds to the designated property.</returns>
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
        public Html this[string name] =>
            new Html { name = this.name.Length == 0 ? this.name.Append(name) : this.name.Append('.').Append(name) };

        /// <summary>
        /// Use expressions to specify the property being bound.
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
        /// </remarks>
        public static Html Bind<T0, T1>(T0 obj, Expression<Func<T0, T1>> prop) =>
            new Html { name = new System.Text.StringBuilder() }.Bind(prop.Body);

        Html Bind(Expression e)
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
                case UnaryExpression u when u.NodeType == ExpressionType.Convert:
                    if (u.Method != null)
                        return u.Method.Invoke(null, new[] { Value(u.Operand) });
                    return Convert.ChangeType(Value(u.Operand), u.Type);
                default: throw new NotSupportedException($"Unrecognized expression type ({e.NodeType}, {e.GetType()}): {e}");
            }
        }

        /// <summary>
        /// Generate an input name.
        /// </summary>
        /// <returns>An input name that binds to the built property.</returns>
        public override string ToString() => name.ToString();
    }
}
