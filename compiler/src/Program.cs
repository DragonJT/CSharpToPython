using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using System.Linq.Expressions;

static class Program{

    static string GetExpression(ExpressionSyntax expression){
        if(expression is InvocationExpressionSyntax invocationExpressionSyntax){
            var output = "";
            output+=GetExpression(invocationExpressionSyntax.Expression);
            output+="(";
            var args = invocationExpressionSyntax.ArgumentList.Arguments.ToArray();
            for(var i=0;i<args.Length;i++){
                output+=GetExpression(args[i].Expression);
                if(i<args.Length-1){
                    output+=",";
                }
            }
            output+=")";
            return output;
        }
        else if(expression is BinaryExpressionSyntax binaryExpressionSyntax){
            return GetExpression(binaryExpressionSyntax.Left)
                +binaryExpressionSyntax.OperatorToken.Text
                +GetExpression(binaryExpressionSyntax.Right);
        }
        else if(expression is LiteralExpressionSyntax literalExpressionSyntax){
            var syntaxkind = literalExpressionSyntax.Kind();
            if(syntaxkind == SyntaxKind.StringLiteralExpression){
                return '"'+literalExpressionSyntax.Token.ValueText+'"';
            }
            return literalExpressionSyntax.Token.ValueText;
        }
        else if(expression is MemberAccessExpressionSyntax memberAccessExpressionSyntax){
            return GetExpression(memberAccessExpressionSyntax)+"."+memberAccessExpressionSyntax.Name.Identifier.Text;
        }
        else if(expression is IdentifierNameSyntax identifierNameSyntax){
            return identifierNameSyntax.Identifier.Text;
        }
        else{
            throw new Exception(expression.GetType().Name);
        }
    }

    static string GetLeadingWS(int depth){
        return new string(' ', depth*4);
    }

    static string GetBlock(StatementSyntax statementSyntax, int depth){
        if(statementSyntax is BlockSyntax block){
            return GetBody(block, depth+1);
        }
        else{
            return GetStatement(statementSyntax, depth+1);
        }
    }

    static string GetStatement(StatementSyntax statement, int depth){
        if(statement is ExpressionStatementSyntax expressionStatementSyntax){
            return GetLeadingWS(depth) + GetExpression(expressionStatementSyntax.Expression)+"\n";
        }
        else if(statement is ForEachStatementSyntax forEachStatementSyntax){
            var output = "";
            output += GetLeadingWS(depth) +"for "+forEachStatementSyntax.Identifier.Text
                +" in "+GetExpression(forEachStatementSyntax.Expression)+":\n";
            output += GetBlock(forEachStatementSyntax.Statement, depth);
            return output;
        }
        throw new Exception(statement.GetType().Name);
    }

    static string GetBody(BlockSyntax block, int depth){
        var output = "";
        foreach(var statement in block.Statements){
            output+=GetStatement(statement, depth);
        }
        return output;
    }

    static string GetTypeName(TypeSyntax typeSyntax)
    {
        return typeSyntax switch
        {
            PredefinedTypeSyntax predefinedType => predefinedType.Keyword.Text,
            IdentifierNameSyntax identifierName => identifierName.Identifier.Text,
            QualifiedNameSyntax qualifiedName => $"{GetTypeName(qualifiedName.Left)}.{qualifiedName.Right.Identifier.Text}",
            _ => typeSyntax.ToString(),
        };
    }

    static void Main(){
        var input = File.ReadAllText("input/Program.cs");
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(input);
        CompilationUnitSyntax root = syntaxTree.GetCompilationUnitRoot();

        var methods = root.DescendantNodes().OfType<MethodDeclarationSyntax>();

        var output = "";
        foreach (var method in methods)
        {
            output+="def "+method.Identifier.Text+"(";
            var parameters = method.ParameterList.Parameters.ToArray();
            for(var i=0;i<parameters.Length;i++){
                output+=parameters[i].Identifier.Text;
                output+=":";
                output+=GetTypeName(parameters[i].Type!);
                if(i<parameters.Length-1){
                    output+=",";
                }
            }
            output+="):\n";
            output+=GetBody(method.Body!, 1);
        }
        output+="Main()\n";
        File.WriteAllText("output/main.py", output);
    }
}
