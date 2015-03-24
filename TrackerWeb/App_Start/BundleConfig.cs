using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Optimization;

namespace TrackerWeb
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                "~/Scripts/jquery.unobtrusive*",
                "~/Scripts/jquery.validate*"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryui").Include(
                "~/Scripts/jquery-ui.js"));

            bundles.Add(new ScriptBundle("~/bundles/datetime").Include(
                "~/Scripts/jquery-ui-sliderAccess.js",
                "~/Scripts/jquery-ui-timepicker-addon.js"));

            bundles.Add(new ScriptBundle("~/bundles/datatables").Include(
                "~/Scripts/jquery.dataTables.js",
                "~/Scripts/dataTables.tableTools.js",
                "~/Scripts/dataTables.editor.js",
                "~/Scripts/dataTables.bootstrap.js",
                "~/Scripts/editorInput.js",
                "~/Scripts/editor.bootstrap.js"));

            bundles.Add(new ScriptBundle("~/bundles/knockout").Include(
                "~/Scripts/knockout-{version}.js",
                "~/Scripts/knockout.validation.js",
                "~/Scripts/betterObservableArray.js"));

            bundles.Add(new ScriptBundle("~/bundles/underscore").Include(
                "~/Scripts/underscore.js"));

            bundles.Add(new ScriptBundle("~/bundles/app").Include(
                "~/Scripts/moment.js",
                "~/Scripts/sammy-{version}.js",
                "~/Scripts/app/common.js",
                "~/Scripts/app/app.datamodel.js",
                "~/Scripts/app/app.viewmodel.js",
                "~/Scripts/app/home.viewmodel.js",
                "~/Scripts/app/_run.js",
                "~/Scripts/app/home.view.js"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                "~/Scripts/bootstrap.js",
                "~/Scripts/respond.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                 "~/Content/jquery-ui.css",
                 "~/Content/theme.css",
                 "~/Content/bootstrap.css",
                 "~/Content/dataTables.tableTools.css",
                 "~/Content/dataTables.bootstrap.css",
                 "~/Content/editor.bootstrap.css",
                 "~/Content/jquery-ui-timepicker-addon.css",
                 "~/Content/Site.css"));
        }
    }
}
