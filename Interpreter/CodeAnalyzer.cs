using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace LisaCore.Interpreter
{
    public static class CodeAnalyzer
    {
        public static List<string> GetNamespaces(string code)
        {
            var tree = CSharpSyntaxTree.ParseText(code);
            var root = tree.GetRoot();
            var usingDirectives = root.DescendantNodes().OfType<UsingDirectiveSyntax>();

            var uniqueNamespaces = new HashSet<string>();

            foreach (var usingDirective in usingDirectives)
            {
                var namespaceName = usingDirective.Name.ToString();
                if (!string.IsNullOrEmpty(namespaceName))
                {
                    uniqueNamespaces.Add(namespaceName);
                }
            }

            return uniqueNamespaces.ToList();
        }
       

    }
}
