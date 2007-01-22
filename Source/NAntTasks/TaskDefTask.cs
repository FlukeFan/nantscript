
using System;
using System.Xml;

using NAnt.Core;
using NAnt.Core.Attributes;
using NAnt.Core.Types;

namespace broloco.NAntTasks
{

    /// <summary>
    /// Allows scripting of a custom task.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///   </para>
    /// </remarks>
    /// <example>
    ///   <para>
    ///   <code>
    ///     <![CDATA[
    ///     ]]>
    ///   </code>
    ///   </para>
    /// </example>
    [TaskName("taskdef")]
    public class TaskDefTask : Task
    {

        private string                  _tagName;
        private StringParamCollection   _stringParams   = new StringParamCollection();
        private NodeParamCollection     _nodeParams     = new NodeParamCollection();
        private RawXml                  _tasks;


        /// <summary>
        /// The name for the custom task.
        /// </summary>
        [TaskAttribute("name", Required=true)]
        public string TagName
        {
            get { return _tagName; }
            set { _tagName = value; }
        }

        /// <summary>
        /// The string parameters.
        /// </summary>
        [BuildElementCollection("stringparams", "stringparam")]
        public StringParamCollection StringParams
        {
            get { return _stringParams; }
        }

        /// <summary>
        /// The xml-node parameters.
        /// </summary>
        [BuildElementCollection("nodeparams", "nodeparam")]
        public NodeParamCollection NodeParams
        {
            get { return _nodeParams; }
        }

        /// <summary>
        /// The tasks to script
        /// </summary>
        [BuildElement("do", Required=true)]
        public RawXml Tasks
        {
            get { return _tasks; }
            set { _tasks = value; }
        }

        /// <summary>
        /// Executes the taskdef task.
        /// </summary>
        protected override void ExecuteTask()
        {
            Log(Level.Info, "Creating task " + _tagName);

            // Generate code for the custom task as a script task ...
            Log(Level.Verbose, "*** Custom code start ***");
            string customTaskCode = "";
            customTaskCode +=  "<script language='C#' >\n";
            customTaskCode +=  "<imports> <import namespace=\"System.Xml\" /> <import namespace=\"NAnt.Core.Types\" /> </imports> <code> <![CDATA[\n";
            customTaskCode +=  "[TaskName(\"" + _tagName + "\")]\n";
            customTaskCode +=  "public class " + _tagName + " : Task\n";
            customTaskCode +=  "{\n";
            customTaskCode +=  "\n";
            customTaskCode +=  "    static private string _originalXml = XmlConvert.DecodeName(\"" + XmlConvert.EncodeLocalName(_tasks.Xml.OuterXml) + "\");\n";
            customTaskCode +=  "\n";

            // generate named string parameters
            foreach (StringParam stringParam in StringParams)
            {
                customTaskCode +=  "    private string _" + stringParam.ParameterName + ";\n";
                customTaskCode +=  "\n";
                customTaskCode +=  "    [TaskAttribute(\"" + stringParam.ParameterName + "\", Required=" + stringParam.Required.ToString().ToLower() + ")]\n";
                customTaskCode +=  "    public string " + stringParam.ParameterName + "\n";
                customTaskCode +=  "    {\n";
                customTaskCode +=  "        get { return _" + stringParam.ParameterName + "; }\n";
                customTaskCode +=  "        set { _" + stringParam.ParameterName + " = value; }\n";
                customTaskCode +=  "    }\n";
                customTaskCode +=  "\n";
            }

            // generate named xml-node parameters
            foreach (NodeParam nodeParam in NodeParams)
            {
                customTaskCode +=  "    private RawXml _" + nodeParam.ParameterName + ";\n";
                customTaskCode +=  "\n";
                customTaskCode +=  "    [BuildElement(\"" + nodeParam.ParameterName + "\", Required=" + nodeParam.Required.ToString().ToLower() + ")]\n";
                customTaskCode +=  "    public RawXml " + nodeParam.ParameterName + "\n";
                customTaskCode +=  "    {\n";
                customTaskCode +=  "        get { return _" + nodeParam.ParameterName + "; }\n";
                customTaskCode +=  "        set { _" + nodeParam.ParameterName + " = value; }\n";
                customTaskCode +=  "    }\n";
                customTaskCode +=  "\n";
            }

            customTaskCode +=  "    protected override void ExecuteTask()\n";
            customTaskCode +=  "    {\n";
            customTaskCode +=  "    }\n";
            customTaskCode +=  "}\n";
            customTaskCode += "]]" + "></code></script>";

            Log(Level.Verbose, customTaskCode);
            Log(Level.Verbose, "*** Custom code end ***");

            XmlDocument xmlScriptTask = new XmlDocument();
            xmlScriptTask.LoadXml(customTaskCode);
            Project.CreateTask(xmlScriptTask.ChildNodes[0]).Execute();
        }

    }
}

