using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using System.Security.Cryptography;

class Class(string name)
{
    public string name = name;
    public List<string> fields = [];
    public List<string> methods = [];
}

class Python{
    readonly List<Class> classStack = [];

    static string GetIdentifier(SyntaxToken identifier){
        var text = identifier.ValueText;
        if(text.StartsWith('@')){
            return text[1..];
        }
        return text;
    }

    static string GetChar(char c){
        if(c=='\n'){
            return "\\n";
        }
        return c.ToString();
    }

    static string GetString(string str){
        string output = "";
        foreach(var c in str){
            output+=GetChar(c);
        }
        return output;
    }

    string GetArguments(ArgumentListSyntax arglist){
        var output="(";
        var args = arglist.Arguments.ToArray();
        for(var i=0;i<args.Length;i++){
            output+=GetExpression(args[i].Expression);
            if(i<args.Length-1){
                output+=",";
            }
        }
        return output+")";
    }

    static string GetFullname(List<string> names){
        string fullname = names[0];
        for(var i=1;i<names.Count;i++){
            fullname = fullname+"."+names[i];
        }
        return fullname;
    }

    string GetMemberOrIdentifierExpression(ExpressionSyntax expression){
        List<string> names = [];
        while(expression is MemberAccessExpressionSyntax memberAccessExpressionSyntax){
            names.Insert(0, GetIdentifier(memberAccessExpressionSyntax.Name.Identifier));
            expression = memberAccessExpressionSyntax.Expression;
        }
        if(expression is IdentifierNameSyntax identifierNameSyntax){
            var name = GetIdentifier(identifierNameSyntax.Identifier);
            var c = classStack[classStack.Count-1];
            if(name == "this"){
                names.Insert(0, "self");
            }
            else if(c.fields.Contains(name) || c.methods.Contains(name)){
                names.Insert(0, "self");
                names.Insert(1, name);
            }
            else{
                names.Insert(0, name);
            }
            return GetFullname(names);
        }
        else{
            return GetExpression(expression) +"."+ GetFullname(names);
        }
    }

    string GetBinaryOperator(SyntaxToken binaryOperator){
        return binaryOperator.ValueText switch{
            "&&" => " and ",
            "||" => " or ",
            _ => binaryOperator.ValueText,
        };
    }

    string GetExpression(ExpressionSyntax expression){
        if(expression is InvocationExpressionSyntax invocationExpressionSyntax){
            return GetExpression(invocationExpressionSyntax.Expression) + GetArguments(invocationExpressionSyntax.ArgumentList);
        }
        else if(expression is CollectionExpressionSyntax collectionExpressionSyntax){
            if(collectionExpressionSyntax.Elements.Count == 0){
                return "[]";
            }
            else{
                throw new Exception(collectionExpressionSyntax.ToFullString());
            }
        }
        else if(expression is ObjectCreationExpressionSyntax objectCreationExpressionSyntax){
            return objectCreationExpressionSyntax.Type.ToString() + GetArguments(objectCreationExpressionSyntax.ArgumentList!);
        }
        else if(expression is ParenthesizedExpressionSyntax parenthesizedExpressionSyntax){
            return "("+GetExpression(parenthesizedExpressionSyntax.Expression)+")";
        }
        else if(expression is BinaryExpressionSyntax binaryExpressionSyntax){
            return GetExpression(binaryExpressionSyntax.Left)
                +GetBinaryOperator(binaryExpressionSyntax.OperatorToken)
                +GetExpression(binaryExpressionSyntax.Right);
        }
        else if(expression is PostfixUnaryExpressionSyntax postfixUnaryExpressionSyntax){
            if(postfixUnaryExpressionSyntax.OperatorToken.ValueText == "++"){
                return GetExpression(postfixUnaryExpressionSyntax.Operand) + "+=1"; 
            }
            else if(postfixUnaryExpressionSyntax.OperatorToken.ValueText == "--"){
                return GetExpression(postfixUnaryExpressionSyntax.Operand) + "-=1"; 
            }
            else{
                throw new Exception(postfixUnaryExpressionSyntax.OperatorToken.ValueText);
            }
        }
        else if(expression is LiteralExpressionSyntax literalExpressionSyntax){
            return literalExpressionSyntax.Kind() switch {
                SyntaxKind.CharacterLiteralExpression => "'" + GetString(literalExpressionSyntax.Token.ValueText) + "'",
                SyntaxKind.StringLiteralExpression => '"' + GetString(literalExpressionSyntax.Token.ValueText) + '"',
                SyntaxKind.TrueLiteralExpression => "True",
                SyntaxKind.FalseLiteralExpression => "False",
                SyntaxKind.NullLiteralExpression => "None",
                _ => literalExpressionSyntax.Token.ValueText
            };
        }
        else if(expression is MemberAccessExpressionSyntax || expression is IdentifierNameSyntax){
            return GetMemberOrIdentifierExpression(expression);
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
        else if(expression is RangeExpressionSyntax rangeExpressionSyntax){
            var left = rangeExpressionSyntax.LeftOperand;
            var right = rangeExpressionSyntax.RightOperand;
            var output = "";
            if(left!=null){
                output+=GetExpression(left);
            }
            output+=":";
            if(right!=null){
                output+=GetExpression(right);
            }
            return output;
        }
        else{
            throw new Exception(expression.GetType().Name);
        }
    }

    string GetLeadingWS(int depth){
        return new string(' ', depth*4);
    }

    string GetBlock(StatementSyntax statementSyntax, int depth){
        if(statementSyntax is BlockSyntax block){
            return GetBody(block, depth+1);
        }
        else{
            return GetStatement(statementSyntax, depth+1);
        }
    }

    string GetElse(ElseClauseSyntax? @else, int depth){
        if(@else != null){
            if(@else.Statement is IfStatementSyntax elseifStatementSyntax){
                return GetLeadingWS(depth) + "elif "+GetExpression(elseifStatementSyntax.Condition)+":\n"
                    + GetBlock(@else.Statement, depth)
                    + GetElse(elseifStatementSyntax.Else, depth);
            }
            else{
                return GetLeadingWS(depth) + "else:\n" + GetBlock(@else.Statement, depth);
            }
        }
        return "";
    }

    string GetStatement(StatementSyntax statement, int depth){
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
                + GetBlock(ifStatementSyntax.Statement, depth)
                + GetElse(ifStatementSyntax.Else, depth);            
        }
        throw new Exception(statement.GetType().Name);
    }

    string GetBody(BlockSyntax block, int depth){
        var output = "";
        foreach(var statement in block.Statements){
            output+=GetStatement(statement, depth);
        }
        return output;
    }

    string GetTypeName(TypeSyntax typeSyntax){
        return typeSyntax switch{
            PredefinedTypeSyntax predefinedType => predefinedType.Keyword.Text,
            IdentifierNameSyntax identifierName => GetIdentifier(identifierName.Identifier),
            QualifiedNameSyntax qualifiedName => $"{GetTypeName(qualifiedName.Left)}.{GetIdentifier(qualifiedName.Right.Identifier)}",
            _ => typeSyntax.ToString(),
        };
    }

    Class GetClass(ClassDeclarationSyntax classDeclarationSyntax){
        var @class = new Class(GetIdentifier(classDeclarationSyntax.Identifier));
        foreach(var m in classDeclarationSyntax.Members){
            if(m is FieldDeclarationSyntax fieldDeclarationSyntax){
                foreach(var f in fieldDeclarationSyntax.Declaration.Variables){
                    @class.fields.Add(GetIdentifier(f.Identifier));
                }
            }
            else if(m is MethodDeclarationSyntax methodDeclarationSyntax){
                @class.methods.Add(GetIdentifier(methodDeclarationSyntax.Identifier));
            }
        }
        return @class;
    }

    string GetMember(MemberDeclarationSyntax member, int depth){
        if(member is ClassDeclarationSyntax classDeclarationSyntax){
            var @class = @GetClass(classDeclarationSyntax);
            classStack.Add(@class);
            var output = GetLeadingWS(depth) + "class "+@class.name+":\n";
            foreach(var m in classDeclarationSyntax.Members){
                output+=GetMember(m, depth+1);
            }
            classStack.RemoveAt(classStack.Count-1);
            return output;
        }
        else if(member is FieldDeclarationSyntax){
            return "";
        }
        else if(member is ConstructorDeclarationSyntax constructorDeclarationSyntax){
            var output = GetLeadingWS(depth)+"def __init__(";
            var parameters = constructorDeclarationSyntax.ParameterList.Parameters.ToArray();
            bool isStatic = constructorDeclarationSyntax.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword));
            if(!isStatic){
                output+="self";
            }
            for(var i=0;i<parameters.Length;i++){
                if(i==0){
                    output+=",";
                }
                output+=GetIdentifier(parameters[i].Identifier);
                /*output+=":";
                output+=GetTypeName(parameters[i].Type!);*/
            }
            output+="):\n";
            output+=GetBody(constructorDeclarationSyntax.Body!, depth+1);
            return output;
        }
        else if(member is MethodDeclarationSyntax methodDeclarationSyntax){
            var output =  GetLeadingWS(depth)+"def "+GetIdentifier(methodDeclarationSyntax.Identifier)+"(";
            var parameters = methodDeclarationSyntax.ParameterList.Parameters.ToArray();
            bool isStatic = methodDeclarationSyntax.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword));
            if(!isStatic){
                output+="self";
            }
            for(var i=0;i<parameters.Length;i++){
                if(i==0){
                    output+=",";
                }
                output+=GetIdentifier(parameters[i].Identifier);
                /*output+=":";
                output+=GetTypeName(parameters[i].Type!);*/
            }
            output+="):\n";
            output+=GetBody(methodDeclarationSyntax.Body!, depth+1);
            return output;
        }
        throw new Exception(member.GetType().Name);
    }

    public string Root(CompilationUnitSyntax root){
        var output = "";
        foreach(var u in root.Usings){
            output+="import "+u.Name!.ToFullString()+"\n";
        }
        foreach(var member in root.Members){
            output+=GetMember(member, 0);
        }
        output+="Program.Main()\n";
        return output;
    }
}

static class Program{
    static void Main(){
        var input = File.ReadAllText("input/Program.cs");
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(input);
        CompilationUnitSyntax root = syntaxTree.GetCompilationUnitRoot();
        var python = new Python();
        try{
            var output = python.Root(root);
            File.WriteAllText("output/main.py", output);
        }
        catch(Exception exception){
            Console.WriteLine(exception);
            File.WriteAllText("output/main.py", "");
        }
    }
}
