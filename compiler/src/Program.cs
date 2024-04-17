using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;

class Method(bool isStatic, string name){
    public bool isStatic = isStatic;
    public string name = name;
}

class Class(string name){
    public string name = name;
    public List<string> fields = [];
    public List<Method> methods = [];
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
        if(c=='\\'){
            return "\\\\";
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
            var name = args[i].NameColon;
            if(name!=null){
                output+=GetExpression(name.Expression)+"=";
            }
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
        if(expression is ThisExpressionSyntax){
            names.Insert(0, "self");
            return GetFullname(names);
        }
        else if(expression is IdentifierNameSyntax identifierNameSyntax){
            var name = GetIdentifier(identifierNameSyntax.Identifier);
            var c = classStack[classStack.Count-1];
            if(c.fields.Contains(name)){ 
                names.Insert(0, "self");
                names.Insert(1, name);
            }
            else if(c.methods.Any(m=>m.name == name)){
                var method = c.methods.First(m=>m.name == name);
                if(method.isStatic){
                    names.Insert(0, c.name);
                }else{
                    names.Insert(0, "self");
                }
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

    string GetExpression(ExpressionSyntax expression, bool isDictionary = false){
        if(isDictionary){
            Console.WriteLine(expression.GetType().Name+"__"+expression.ToFullString());
        }
        if(expression is InvocationExpressionSyntax invocationExpressionSyntax){
            return GetExpression(invocationExpressionSyntax.Expression) + GetArguments(invocationExpressionSyntax.ArgumentList);
        }
        else if(expression is CollectionExpressionSyntax collectionExpressionSyntax){
            var elements = collectionExpressionSyntax.Elements.ToArray();
            var output = "[";
            for(var i=0;i<elements.Length;i++){
                if(elements[i] is ExpressionElementSyntax expressionElementSyntax){
                    output+=GetExpression(expressionElementSyntax.Expression);
                }
                else{
                    throw new Exception(elements[i].GetType().Name);
                }
                if(i<elements.Length-1){
                    output+=",";
                }
            }
            return output+"]";
        }
        else if(expression is InitializerExpressionSyntax initializerExpressionSyntax){
            var output = "{";
            var expressions = initializerExpressionSyntax.Expressions.ToArray();
            for(var i=0;i<expressions.Length;i++){
                output+=GetExpression(expressions[i], true);
                if(i<expressions.Length-1){
                    output+=",";
                }
            }
            return output+"}";
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
            var op = assignmentExpressionSyntax.OperatorToken.ValueText;
            if(isDictionary && assignmentExpressionSyntax.OperatorToken.ValueText=="="){
                op = ":";
            }
            return GetExpression(assignmentExpressionSyntax.Left)+op+GetExpression(assignmentExpressionSyntax.Right);
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
                    + GetBlock(@elseifStatementSyntax.Statement, depth)
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
        else if(statement is ReturnStatementSyntax returnStatementSyntax){
            var rtnExpression = returnStatementSyntax.Expression!=null?" "+GetExpression(returnStatementSyntax.Expression):"";
            return GetLeadingWS(depth) + "return"+rtnExpression+"\n";
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
                bool isStatic = methodDeclarationSyntax.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword));
                @class.methods.Add(new Method(isStatic, GetIdentifier(methodDeclarationSyntax.Identifier)));
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
                if(!(isStatic && i==0)){
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
                if(!(isStatic && i==0)){
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

    public string CompileRoot(CompilationUnitSyntax root){
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

    static void Run(string input, string output){
        var code = File.ReadAllText(input);
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(code);
        CompilationUnitSyntax root = syntaxTree.GetCompilationUnitRoot();
        var python = new Python();
        try{
            File.WriteAllText(output, "#SUCCESS\n"+python.CompileRoot(root));
        }
        catch(Exception exception){
           var lines = ("#ERROR\n"+exception).ToString().Split('\n').Select(l=>'#'+l).ToArray();
           File.WriteAllLines(output, lines);
        }
    }

    static void Main(string[] args){
        var finalArgs = args[0].Split("--");
        Run(finalArgs[0], finalArgs[1]);
    }
}
