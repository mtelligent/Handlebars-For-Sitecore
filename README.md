# Handlebars for Sitecore

Welcome to Handlebars for Sitecore, a deep extensible set of features that allow you to define presentation markup within Handlebar Templates stored in Sitecore.

If you are using SXA, you can activate the Handlebars feature on your site to get a set of Handlebar Components in the toolbox, or take advantage of the Handlebar Variant field type to define your Variant markup using Handlebar Syntax.

If you're not using SXA, just add our Handlebar components to your placeholder settings to make them available for use. 

# Installation:

Check out the [releases page](https://github.com/mtelligent/Handlebars-For-Sitecore/releases) for the latest installation packages. You'll find the core Handlebars for Sitecore Package and an additional SXA package that you only need to install if you're working with SXA.

The current release has been tested with Sitecore 9.1 and SXA 1.8. 

# How it works

Handlebar templates are defined in Sitecore Items. The templates themselves are editable using an auto complete friendly code editor, making it easy to build complex templates cleanly.

Templates are bound to Sitecore Items server side. The components and variants are normal Sitecore components and can be configued for personalization, AB Testing and Caching setting as needed. Because you have separate components for context (which items to bind) and presentation (the templates themselves) you get the fleixbility to easily personlize or test either the data sources or presentation very easily.

# Binding to Sitecore Items

To make binding easy, context data sources are wrapped in a Dynamic Item, which wraps each Sitecore Item and adds additional properties that can be accessed as Handlebar fields. 

Every Field of your item can be Accessed through the Dynamic Item as the field name. The rendered value of the field will go through the standard field renderer, and be editable. To properly support field editor, we recommend using the html encoded handlebar syntax with three braces. For example to render a field called "Title" as editable, simply use:

```
{{{ Title }}}
```

Besides Field Names, we've added common properties to make it easy to use:
- IsExperienceEditorEditing – returns true when in experience editor.
- ItemUrl – Url of the context item
- ItemId – Item id of the context item
- Children - collection of children of the item
- Parent - Parent of the Item.

Additionally, we make it easy to access properties of the field without going through helper functions by appending an underscore and suffix to the field name. For example if you were trying to access the value of a field named "Title" without going through the field renderer, you could use the following syntax:

```
{{ Title_Value }}
```

The other global suffix is "Field", which gives you access to the actual field object. This is really only useful when passing it to a helper function.

Depending on the underlying Field Type we add additional helper suffix properties on the fly. Just append an underscore after the field name to access it.

- LinkField
  - FieldName_Url – returns friendly url stored in the field 
  - FieldName_Text – returns the test description of the link field
- ImageField
  - FieldName_Src – returns the raw Media Library url
  - FieldName_Alt – Returns the configured Alt Text for the Media Item
- Date, DateTime (DateField)
  - FieldName_ShortDate – returns the date formatted ToShortDateString
  - FieldName_LongDate – returns the date formatted ToLongDateString
 
Multilist Fields 
  - FieldName_Count – Returns number of items in the list.

Note that both Multilist and certain link fields (Droplink, DropTree, Grouped Drop Link) do not go through the field renderer, but are automatically converted to arrays of dynamic items, allowing you to iterate or navigate to properties of selected items.

# The Containers

When using SXA Variants, the template will bind to the item provided by SXA for binding. 

When using components, you need to use a "container component" to define the context and a handlebar template component to define the presentation.

There are 3 main Sitecore Data Container Components:
- Handlebar Container – Set the Data Source to the Item you want to bind to. Leave blank to allow binding to the Context Item.
- Handlebar Collection – Allows you to specify a folder or collection of Items to be bound. "Items" is the top most collection for binding.
- Handlebar Query – Will use a Sitecore query to fetch items to be bound based on Rendering Parameters. "Items" is the top most collection for binding.

The query container is pretty flexible. We added some syntax helpers to the Field Filters section to make it easier to define generic rules including:
- Negation – Prefix the value with ! if you want the filter to say “not equal to” instead of the default equal to.
- Match the ID of Sitecore Context Item – Use a value of $ID if you want a field on the template to match the ID of the current page
- Match a value of a field on the Context Item. Use a value of $ followed by the field name to compare a field on the bound item to a field on the page Item.

Keep in mind your search index needs to be up to date to leverage the query container. 

There are two other containers that can bind to other kinds of data:
- Facet Container - Allows you to bind to any facet (OOB or Custom) to easily surface facet data.
- JSON Rest Container - can bind any restful “get” url that you configure it to a template.


# Handlebar Helpers

You can even register your own c# functions to add additional functionality. Each function needs to be registered in the handelbar helpers pipeline. 

There are two kinds of helpers, one that produces output:

```
    public class RegisterTranslateHelper
    {
        public void Process(HandlebarHelpersPipelineArgs pipelineArgs)
        {
            pipelineArgs.Helpers.Add(new HandlebarHelperRegistration("translate", (writer, context, args) =>
            {
                string input = args[0].ToString();
                var translated = Sitecore.Globalization.Translate.Text(input);
                if (!string.IsNullOrEmpty(translated))
                {
                    writer.Write(translated);
                }
                else
                {
                    writer.Write("<!-- Nothing Found for Key: " + input + " -->");
                }

            }));
        }
    }
```

And one that allows you to define a condition that can be used like an if else block.

```
    public class RegisterIfEqualHelper
    {
        public void Process(HandlebarHelpersPipelineArgs pipelineArgs)
        {
            pipelineArgs.Helpers.Add(new HandlebarHelperRegistration("ifEqual", (writer, options, context, args) =>
            {
                if (args[0] == args[1] || args[0].Equals(args[1]))
                {
                    options.Template(writer, (object)context);
                }
                else
                {
                    options.Inverse(writer, (object)context);
                }
            }));
        }
    }
```

After defining your class, you just need to register it in the Handlebar helpers pipeline.

```
<configuration xmlns:patch="http://www.sitecore.net/xmlconfig/">
  <sitecore>
    <pipelines>
      <handlebarHelpers>
        <processor type="SF.Foundation.Handlebars.RegisterFormatDateHelper, SF.Foundation.Handlebars" />
        <processor type="SF.Foundation.Handlebars.RegisterIfEqualHelper, SF.Foundation.Handlebars" />
        <processor type="SF.Foundation.Handlebars.RegisterJSONHelper, SF.Foundation.Handlebars" />
        <processor type="SF.Foundation.Handlebars.RegisterPlaceholderHelper, SF.Foundation.Handlebars" />
        <processor type="SF.Foundation.Handlebars.RegisterUserSettingsHelper, SF.Foundation.Handlebars" />
        <processor type="SF.Foundation.Handlebars.RegisterIfNotEqualHelper, SF.Foundation.Handlebars" />
        <processor type="SF.Foundation.Handlebars.RegisterRenderFieldWithParameters, SF.Foundation.Handlebars" />
        <processor type="SF.Foundation.Handlebars.RegisterEditFrameHelper, SF.Foundation.Handlebars" />
      </handlebarHelpers>
    </pipelines>
  </sitecore>
</configuration>
```

Out of the box, we support several helper functions including:

- formatDate - supply a date format string to control date output.
- ifEqual - compare two values
- json - serialize an item or collection to json
- placeholder - embed a dynamic placeholder in a template
- userSettings - retrieves a value from current users user setting facet (more on this later)
- ifNotEqual - inverse of ifEqual
- fieldRenderer - allows you to pass arguments to the filed renderer
- editFrame - allows you to embed an edit frame within a template

# Handlebar Forms

Handlebar Forms are a component that allows you to define form markup completley in a handlbar template, binding to a content item for form labels and thank you messaging. In addition, it provides an interface for building form processors which can handle the data. So if you have specific logic in mind, you can define your own processor. Or you can use one of the two processors that come with the framework. 
- The SQL Form Processor allows you to map form parameters to SQL command parameters to insert records into a Database table. 
- The User Settings Form Processor leverages User Settings Facets, which allow you to store data in xDB in a custom facet for storing user specific key value pairs (more on this in the future).

# User Settings

The Foundation Facets project is also included with handlebars for sitecore, to support binding to facets and saving data in handlebar forms. This is a generic facet that stores a dictionary of dictionaries. Allowing you to store key value pair collections for a user in xDB. This is convenient for data that doesn't need to be highly structured, and it's pretty easy to work with as a Facade is provided that hides the Facet storage code.

```
\\get or set setting on default area.
UserSettings.Settings[key];

\\get or set setting in specific area
UserSettings.Settings[key, area];
```

It even comes with a Personalization Condition that allows you configure a rule based on one of the settings.

# Samples

**Using Placeholders**
```
<div class="row">
<div class="col-md-6">
{{{ placeholder 'AirspaceLeft' }}}
</div>
<div class="col-md-6">
{{{ placeholder 'AirspaceRight' }}}
</div>
</div>
```

**ifEqual**
Pass it two properties (one can be a property of the context Item). Note this is a Block Helper, as compared to the otehr which is a normal helper.

```
{{#ifEqual 'test' 'test2'}}
They are equal
{{else}}
They are not equal
{{/ifEqual}}
```

**formatDate**  
Parses first value as date then formats to String with second param
`{{ formatDate Date_Value 'MM/yyyy' }}`


**Single Item:**

```
<div class="blog-post">
<h1>{{{Title}}} <small>{{Date_ShortDate}}</small></h1>
<div class="thumbnail">{{{Image}}}</div>
<div>{{{Summary}}}</div>
<div>{{{Content}}}</div>
<div class="panel callout">
<ul class="menu simple">
    <li><a href="#">Author: {{{Author.Name}}}</a></li>
</ul>
</div>
</div>
```


**Simple List:**

```
{{ #if IsExperienceEditorEditing  }}
  <div>Experience Editor Mode</div>
{{ /if }}
{{ #each Items }}
  <div class="blog-post">
  {{ #if IsExperienceEditorEditing  }}
    <h3>{{{Title}}} <small>{{Date_ShortDate}}</small></h3>
  {{else}}
    <h3><a href="{{ItemUrl}}">{{{Title}}}</a> <small>{{Date_ShortDate}}</small></h3>
  {{ /if }}
  <div class="thumbnail">{{{Image}}}</div>
  <div>{{{Summary}}}</div>
  <div class="panel"><small><a href="{{{ Author.ItemUrl }}}">by {{{Author.Name}}}</a><br />
  <a href="{{{ Category.ItemUrl }}}">Posted In {{{Category.Name}}}</a></small></div>
  </div>
{{ /each }}
```

**Carousel:** 

```
<div class="owl-carousel owl-theme" data-options="{ &quot;singleItem&quot;:true,&quot;autoPlay&quot;:true,&quot;transitionStyle&quot;:&quot;fade&quot;}
{{ #each Items }}
  <div class="carousel-Item">
  <div class="row">
    <div class="large-5  small-5 medium-5 columns">{{{Image}}}</div>
    <div class="large-7  small-7 medium-7 columns">
    {{ #if IsExperienceEditorEditing  }}
      <h3>{{{Title}}} <small>{{Date_ShortDate}}</small></h3>
    {{else}}
      <h3><a href="{{ItemUrl}}">{{{Title}}}</a> <small>{{Date_ShortDate}}</small></h3>
    {{ /if }}
    <div>{{{Summary}}}</div>
    <div class="panel"><small><a href="{{{ Author.ItemUrl }}}">by {{{Author.Name}}}</a><br />
    <a href="{{{ Category.ItemUrl }}}">Posted In {{{Category.Name}}}</a></small></div>
  </div>
  </div>
  </div>
{{ /each }}
</div>
```

