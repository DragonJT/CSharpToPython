using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;

static class Program{

    static string GetIdentifier(SyntaxToken identifier){
        var text = identifier.ValueText;
        if(text.StartsWith('@')){
            return text[1..];
        }
        return text;
    }

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
            return literalExpressionSyntax.Kind() switch
            {
                SyntaxKind.StringLiteralExpression => '"' + literalExpressionSyntax.Token.ValueText + '"',
                SyntaxKind.TrueLiteralExpression => "True",
                SyntaxKind.FalseLiteralExpression => "False",
                _ => literalExpressionSyntax.Token.ValueText
            };
        }
        else if(expression is MemberAccessExpressionSyntax memberAccessExpressionSyntax){
            return GetExpression(memberAccessExpressionSyntax.Expression)+"."+GetIdentifier(memberAccessExpressionSyntax.Name.Identifier);
        }
        else if(expression is IdentifierNameSyntax identifierNameSyntax){
            return GetIdentifier(identifierNameSyntax.Identifier);
        }
        else if(expression is AssignmentExpressionSyntax assignmentExpressionSyntax){
            return GetExpression(assignmentExpressionSyntax.Left)
                +assignmentExpressionSyntax.OperatorToken.ValueText
                +GetExpression(assignmentExpressionSyntax.Right);
        }
        else if(expression is ElementAccessExpressionSyntax elementAccessExpressionSyntax){
            return GetExpression(elementAccessExpressionSyntax.Expression)+"["
                +GetExpression(elementAccessExpressionSyntax.ArgumentList.Arguments[0].Expression)+"]";
        }
        else if(expression is TupleExpressionSyntax tupleExpressionSyntax){
            var output = "(";
            var args = tupleExpressionSyntax.Arguments.ToArray();
            for(var i=0;i<args.Length;i++){
                output+=GetExpression(args[i].Expression);
                if(i<args.Length-1){
                    output+=",";
                }
            }
            return output+")";
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
            return GetLeadingWS(depth) +"for "+GetIdentifier(forEachStatementSyntax.Identifier)
                + " in "+GetExpression(forEachStatementSyntax.Expression)+":\n"
                + GetBlock(forEachStatementSyntax.Statement, depth);
        }
        else if(statement is LocalDeclarationStatementSyntax localDeclarationStatementSyntax){
            var variables = localDeclarationStatementSyntax.Declaration.Variables.ToArray();
            return GetLeadingWS(depth) + GetIdentifier(variables[0].Identifier) + "="+ GetExpression(variables[0].Initializer!.Value)+"\n";
        }
        else if(statement is WhileStatementSyntax whileStatementSyntax){
            return GetLeadingWS(depth) +"while "+GetExpression(whileStatementSyntax.Condition)+":\n"
                + GetBlock(whileStatementSyntax.Statement, depth);
        }
        else if(statement is IfStatementSyntax ifStatementSyntax){
            return GetLeadingWS(depth) + "if "+GetExpression(ifStatementSyntax.Condition)+":\n"
                + GetBlock(ifStatementSyntax.Statement, depth);
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
            IdentifierNameSyntax identifierName => GetIdentifier(identifierName.Identifier),
            QualifiedNameSyntax qualifiedName => $"{GetTypeName(qualifiedName.Left)}.{GetIdentifier(qualifiedName.Right.Identifier)}",
            _ => typeSyntax.ToString(),
        };
    }

    static string GetMember(MemberDeclarationSyntax member){
        if(member is ClassDeclarationSyntax classDeclarationSyntax){
            var output = "";
            foreach(var m in classDeclarationSyntax.Members){
                output+=GetMember(m);
            }
            return output;
        }
        else if(member is MethodDeclarationSyntax methodDeclarationSyntax){
            var output = "def "+GetIdentifier(methodDeclarationSyntax.Identifier)+"(";
            var parameters = methodDeclarationSyntax.ParameterList.Parameters.ToArray();
            for(var i=0;i<parameters.Length;i++){
                output+=GetIdentifier(parameters[i].Identifier);
                output+=":";
                output+=GetTypeName(parameters[i].Type!);
                if(i<parameters.Length-1){
                    output+=",";
                }
            }
            output+="):\n";
            output+=GetBody(methodDeclarationSyntax.Body!, 1);
            return output;
        }
        throw new Exception(member.GetType().Name);
    }

    static void Main(){
        var input = File.ReadAllText("input/Program.cs");
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(input);
        CompilationUnitSyntax root = syntaxTree.GetCompilationUnitRoot();
        var output = "";
        foreach(var u in root.Usings){
            output+="import "+u.Name!.ToFullString()+"\n";
        }
        foreach(var member in root.Members){
            output+=GetMember(member);
        }
        output+="Main()\n";
        File.WriteAllText("output/main.py", output);
    }
}
