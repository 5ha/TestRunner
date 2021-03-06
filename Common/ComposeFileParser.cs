﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YamlDotNet.RepresentationModel;

namespace Common
{
    public class ComposeFileParser
    {
        private readonly string _yamlText;
        private readonly YamlStream _yaml;


        public ComposeFileParser(string yamlText)
        {
            _yamlText = yamlText;

            _yaml = new YamlStream();
            using (TextReader sr = new StringReader(yamlText))
            {
                _yaml.Load(sr);
            }
        }

        public void AddEnvironmentVariables(Dictionary<string, string> environmentVariables)
        {
            var environmentNode = GetNode<YamlSequenceNode>("environment", TesterNode);

            if (environmentNode == null)
            {
                environmentNode = new YamlSequenceNode();
                TesterNode.Add(new YamlScalarNode("environment"), environmentNode);
            }

            foreach(string key in environmentVariables.Keys)
            {
                environmentNode.Add(new YamlScalarNode($"{key}={environmentVariables[key]}"));
            }
        }

        public string Save()
        {
            StringBuilder s = new StringBuilder();
            using (TextWriter tw = new StringWriter(s))
            {
                _yaml.Save(tw);
            }
            return s.ToString();
        }

        public string GetTesterImageName()
        {
            return ((YamlScalarNode)ImageNode).Value;
        }

        public string GetTesterLocation()
        {
            if (CommandNode.NodeType == YamlNodeType.Scalar)
            {
                YamlScalarNode n = CommandNode as YamlScalarNode;
                return GetLocationString(n.Value);
            }

            if (CommandNode.NodeType == YamlNodeType.Sequence)
            {
                YamlSequenceNode n = CommandNode as YamlSequenceNode;
                YamlScalarNode child = n.Children[0] as YamlScalarNode;
                return GetLocationString(child.Value);
            }

            return null;
        }

        private string GetLocationString(string input)
        {
            Match m = Regex.Match(input, @"[a-zA-Z0-9\\/:]*RunTests\.exe");

            return m.Value;
        }

        private YamlMappingNode _serviceNode;
        public YamlMappingNode ServiceNode
        {
            get
            {
                if (_serviceNode == null)
                {
                    _serviceNode = GetNode<YamlMappingNode>("services");
                }

                if (_serviceNode == null) {
                    throw new InvalidDataException("Cannot find node 'services' in YAML document");
                }

                return _serviceNode;
            }
        }

        private YamlMappingNode _testerNode;
        public YamlMappingNode TesterNode
        {
            get
            {
                if (_testerNode == null)
                {
                    _testerNode = GetNode<YamlMappingNode>("tester", ServiceNode);
                }

                if (_testerNode == null)
                {
                    throw new InvalidDataException("Cannot find node 'services/tester' in YAML document");
                }

                return _testerNode;
            }
        }

        private YamlNode _commandNode;
        public YamlNode CommandNode
        {
            get
            {
                if (_commandNode == null)
                {
                    _commandNode = GetNode<YamlNode>("command", TesterNode);
                }

                if (_commandNode == null)
                {
                    throw new InvalidDataException("Cannot find node 'services/tester/command' in YAML document");
                }

                return _commandNode;
            }
        }

        private YamlNode _imageNode;
        public YamlNode ImageNode
        {
            get
            {
                if (_imageNode == null)
                {
                    _imageNode = GetNode<YamlNode>("image", TesterNode);
                }

                if(_imageNode == null)
                {
                    throw new InvalidDataException("Cannot find node 'services/tester/image' in YAML document");
                }

                return _imageNode;
            }
        }

        private T GetNode<T>(string tag, YamlMappingNode ctx = null) where T : YamlNode
        {
            if (ctx == null)
                ctx = (YamlMappingNode)_yaml.Documents[0].RootNode;

            try
            {
                var n = ctx.Children[new YamlScalarNode(tag)];
                return n as T;
            }
            catch
            {
                return null;
            }
        }
    }
}
