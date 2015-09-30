using System;
using EnvDTE;
using Microsoft.VisualStudio.VCProjectEngine;

namespace CppAutoLib
{

    public static class Extensions
    {

        public static void AddLinkerInput(this Project project, string lib)
        {
            var vcp = project.Object as VCProject;
            var tools = vcp.ActiveConfiguration.Tools as IVCCollection;
            var linkerTool = tools.Item("VCLinkerTool") as VCLinkerTool;
            linkerTool.AdditionalDependencies = lib + " " + linkerTool.AdditionalDependencies;
        }

        public static void Build(this Project project)
        {
            var vcp = project.Object as VCProject;

            try
            {
                vcp.ActiveConfiguration.Build();
                vcp.ActiveConfiguration.WaitForBuild();
            }
            catch (Exception)
            {
                
            }
        }

    }

}