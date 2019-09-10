using System.Web.Mvc;
using System.Web.Optimization;



//NOTE:--    Please replace ViennaAdvantage with prefix of your module..



namespace ViennaAdvantage //  Please replace namespace with prefix of your module..
{
    public class ViennaAdvantageAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "ViennaAdvantage";   //Please replace "ViennaAdvantage" with prefix of your module.......
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "ViennaAdvantage_default",
                "ViennaAdvantage/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
                , new[] { "ViennaAdvantage.Controllers" }
            );    // Please replace ViennaAdvantage with prefix of your module...


            StyleBundle style = new StyleBundle("~/Areas/ViennaAdvantage/Contents/ViennaAdvantageStyle");
            ScriptBundle script = new ScriptBundle("~/Areas/ViennaAdvantage/Scripts/ViennaAdvantageJs");
            /* ==>  Here include all css files in style bundle......see example below....  */

            //style.Include("~/Areas/ViennaAdvantage/Contents/example1.css",
            //              "~/Areas/ViennaAdvantage/Contents/example2.css");

           // script.Include("~/Areas/ViennaAdvantage/Scripts/apps/Framework/infoproduct.js",
           //             "~/Areas/ViennaAdvantage/Scripts/apps/Framework/infoscanform.js",
            //              "~/Areas/ViennaAdvantage/Scripts/apps/Framework/pattributesform.js");


            script.Include("~/Areas/ViennaAdvantage/Scripts/ViennaAdvantage.all.min.js");


            /*-------------------------------------------------------
              Please replace "ViennaAdvantage" with prefix of your module..
             * 
             * 1. first parameter is script/style bundle...
             * 
             * 2. Second parameter is module prefix...
             * 
             * 3. Third parameter is order of loading... (dafault is 10 )
             * 
             --------------------------------------------------------*/

            VAdvantage.ModuleBundles.RegisterScriptBundle(script, "ViennaAdvantage", 10);
            VAdvantage.ModuleBundles.RegisterStyleBundle(style, "ViennaAdvantage", 10);
        }
    }
}