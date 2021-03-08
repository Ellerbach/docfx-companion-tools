# UI specific elements for DocFX

## Serving specific files

If you start customizing the DocFX templates, you'll most likely end up having to support specific files for the search and for the fonts. Depending where you deploy them, this may require to adjust your web server. In the case of deployment in an Azure Web Application, you will need to adjust the [web.config](../ui-specific/web.config) file.

You will have to add it to the file you deploy as well in the root directory. See how it's done in this example in the [docfx.json](../docfx.json) file.

## Adding support for mermaid schemas

[Mermaid](https://github.com/mermaid-js/mermaid) is a nice way to get support for relational drawing, flowchart and diagram out of text. It is not supported out of the box by DocFX, you need to add this support. One of the best way is to customize the UI template and adjust the file in `partials/scripts.tmpl.partial` like that:

```js
{{!Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license. See LICENSE file in the project root for full license information.}}

<script type="text/javascript" src="{{_rel}}styles/docfx.vendor.js"></script>
<script type="text/javascript" src="{{_rel}}styles/docfx.js"></script>
<script type="text/javascript" src="{{_rel}}styles/main.js"></script>

<!-- mermaid support -->
<script src="https://unpkg.com/mermaid@8.4/dist/mermaid.min.js"></script>
<script>
    mermaid.initialize({
        startOnLoad: false
    });

    window.onload = function () {
        const elements = document.getElementsByClassName("lang-mermaid");
        let index = 0;

        while (elements.length != 0) {
            let element = elements[0];
            mermaid.render('graph'+index, element.innerText, (svgGraph) => {                    
                element.parentElement.outerHTML = "<div class='mermaid'>" + svgGraph + "</div>";
            });
            ++index;
        }
    }
</script>
```

*Note*: like any other script, please don't forget to update the version regularly.

Here is a very simple mermaid example:

```mermaid
requests
| where name == "POST Idoc/Send"
| where resultCode == 200
| where customDimensions contains "ResponseBody"
| order by timestamp desc
```
