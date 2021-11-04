using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace WebForms.Binding
{
    public static class NameBinding
    {
        public static Input BindTo<T0, T1>(this T0 obj, Expression<Func<T0, T1>> prop) =>
            Input.Name(obj, prop);
    }
}
