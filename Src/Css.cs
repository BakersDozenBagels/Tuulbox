﻿using RT.Servers;
using RT.Util.ExtensionMethods;

namespace Tuulbox
{
    sealed class Css : ITuul
    {
        private static byte[] _css = @"

body {
    background: #eee;
    margin: -10px 0 0 0;
    font-family: ""Candara"", ""Calibri"", ""Tahoma"", ""Verdana"", ""Arial"", sans-serif;
}
.everything {
    max-width: 50em;
    margin: 0 auto 20px;
    background: white;
    box-shadow: 0 0 5px rgba(0, 0, 0, .5);
    padding: 3em;
    border-radius: 7px;
}
.footer {
    padding: .5em;
    text-align: center;
}
h1 {
    font-variant: small-caps;
    font-size: 47pt;
}
div.search {
    float: right;
}
.content {
    border-top: 1px solid #ccc;
    padding-top: 2em;
}
.content h2 {
    font-size: 24pt;
    font-variant: small-caps;
}
textarea {
    width: 100%;
    height: 15em;
}
.tuulname { font-weight: bold; }
.explain { color: #888; margin: .2em 0 .7em 2em; font-size: 80%; }

table.layout { table-layout: fixed; width: 100%; border-collapse: collapse; }
table.layout th, table.layout td { vertical-align: top; }

kbd.accesskey { font-family: inherit; text-decoration: underline; }

pre { margin: 0; }

.tab-control > .tab {
    border: 1px solid #888;
    border-radius: 5px;
    overflow: auto;
    padding: .5em .7em;
}
.tab-control > .tabs {
    padding-left: .5em;
}
.tab-control > .tabs > a {
    display: inline-block;
    border: 1px solid #ccc;
    border-bottom: none;
    border-top-left-radius: 10px;
    border-top-right-radius: 10px;
    padding: .2em .5em;
    vertical-align: bottom;
}
.tab-control > .tabs > a.selected {
    border-color: #888;
    padding: .3em .7em;
    background: white;
    position: relative;
    top: 1px;
}

.error {
    background: #fff6ee;
    border: 1px solid #f82;
    padding: .5em 1em;
    position: relative;
}
.error:before {
    position: absolute;
    right: 0;
    top: 0;
    background: #f82;
    content: 'Error';
    padding: .1em .5em;
    color: white;
    font-weight: bold;
}
.error .input { margin-bottom: .7em; position: relative; color: black; font-size: 250%; white-space: pre; }
.error .good { color: #082; }
.error .bad { color: #a24; }
.error .rest { color: #ddd; }
.error .indicator { color: #a24; position: absolute; font-size: 70%; }
.error .indicator:before { content: '^'; position: relative; left: -50%; top: 1.4em; }

code { background: rgba(0, 0, 0, .05); padding: .05em .2em; border: 1px solid rgba(0, 0, 0, .1); }
".ToUtf8();

        object ITuul.Handle(TuulboxModule module, HttpRequest req)
        {
            return HttpResponse.Css(_css);
        }

        bool ITuul.Enabled { get { return true; } }
        bool ITuul.Listed { get { return false; } }
        string ITuul.Name { get { return null; } }
        string ITuul.UrlName { get { return "css"; } }
        string ITuul.Keywords { get { return null; } }
        string ITuul.Description { get { return null; } }
        string ITuul.Js { get { return null; } }
        string ITuul.Css { get { return null; } }
    }
}
