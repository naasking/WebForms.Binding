# WebForms.Binding

This is a tiny abstraction to enable full model binding with the old ASP.NET web forms,
in case you're unfortunate enough to still be working with those.

It provides a fast low-level interface for generating input names, and an easier high-level
interface that's likely considerably slower.

It's not quite as easy as model binding under MVC, but it at least enables you to use web forms as a templating
language for better HTML and JavaScript integration, and lets you get rid of all of that viewstate.

# Example View Models

I'll use the following view models in the examples below:

    public class PresentationViewModel
    {
        public int Id { get; set; }
        [Required]
        public DateTime? PresentationDate { get; set; }
        public RegistrationViewModel[] Attendance { get; set; }
    }
    public class RegistrationViewModel
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public bool? Present { get; set; }
    }

The code behind for the .aspx might look like:

    public partial class PresentationView : System.Web.UI.Page
    {
        protected PresentationViewModel model;

        protected override void OnLoad(EventArgs e)
        {
            // load the view model from some backing store
            var id = Request.QueryString["id"];
            model = GetDataStore().LoadPresentationViewModel(id);

            // apply any updates via model binding on every postback
            if (IsPostBack)
                TryUpdateModel(model, new FormValueProvider(this.ModelBindingExecutionContext));
            base.OnLoad(e);
        }
        
        protected override void OnPreRender(EventArgs e)
        {
            DataBind();
            base.OnPreRender(e);
        }
    }

So the page loads a pre-existing view model for a scheduled presentation. After load, any registered
event handlers are run, and then the whole page is data bound on pre-render. I've found this
pattern to work fairly well for most scenarios.

# High-Level API

We can bind to it as follows:

    <h1>My Presentation</h1>
    <div><input name="<%# model.BindTo(x => x.PresentationDate) %>" type="datetime" value="<%# model.PresentationDate?.ToString("yyyy-MM-dd") %>" required /></div>
    
    <h2>Attendance</h2>
    <asp:Repeater runat="server" DataSource="<%# model.Attendance %>" ItemType="MyProject.RegistrationViewModel">
        <ItemTemplate>
            <label>
                <input type="hidden" name="<%# model.BindTo(x => x.Attendance[Container.ItemIndex].Id) %>" value="<%# Item.Id %>" />
                <input type="text" name="<%# model.BindTo(x => x.Attendance[Container.ItemIndex].Name) %>" value="<%# Item.Name %>" required />
                <input type="checkbox" name="<%# model.BindTo(x => x.Attendance[Container.ItemIndex].Present) %>" value="<%# Item.Present %>"
                    <%# Html.Checked(Item.Present == true) %> onchange="__doPostBack()" />
            </label>
        </ItemTemplate>
    </asp:Repeater>

# Low-Level API

The low-level API provides maximum performance but is more verbose. Given the same classes as above, using the
low-level API looks like:

    <h1>My Presentation</h1>
    <div><input name="<%# Input.Name[nameof(model.PresentationDate)] %>" type="datetime" value="<%# model?.ToString("yyyy-MM-dd") %>" required /></div>
    
    <h2>Attendance</h2>
    <asp:Repeater runat="server" DataSource="<%# model.Attendance %>" ItemType="MyProject.RegistrationViewModel">
        <ItemTemplate>
            <label>
                <input type="hidden" name="<%# Input.Name[nameof(model.Attendance)][Container.ItemIndex][nameof(ChildEntity.Id)] %>" value="<%# Item.Id %>" />
                <input type="text" name="<%# Input.Name[nameof(model.Attendance)][Container.ItemIndex][nameof(ChildEntity.Name)] %>" value="<%# Item.Name %>" required />
                <input type="checkbox" name="<%# Input.Name[nameof(model.Attendance)][Container.ItemIndex][nameof(ChildEntity.Present)] %>" value="<%# Item.Present %>"
                    <%# Html.Checked(Item.Present == true) %> onchange="__doPostBack()" />
            </label>
        </ItemTemplate>
    </asp:Repeater>

Each component of the path needed to access the child element must be added to the name, which you can do
via indexing integers or property names. You can use the `nameof()` expression so the name used 
corresponds exactly with the code's property name.
