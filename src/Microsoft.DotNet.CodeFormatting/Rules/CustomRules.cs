// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.DotNet.CodeFormatting.Rules
{
    /// <summary>
    /// Ensure there is a blank line above the first using and namespace in the file. 
    /// </summary>
    [SyntaxRule(CustomRules.Name, CustomRules.Description, SyntaxRuleOrder.CustomRules)]
    internal sealed class CustomRules : CSharpOnlyFormattingRule, ISyntaxFormattingRule
    {
        internal const string Name = "Custom";
        internal const string Description = "Apply custom coding rules.";

        public SyntaxNode Process(SyntaxNode syntaxRoot, string languageName)
        {
            return new CustomRuleRewriter().Visit(syntaxRoot);
        }
    }

    internal sealed class CustomRuleRewriter : CSharpSyntaxRewriter
    {
        public override SyntaxNode VisitConditionalExpression(ConditionalExpressionSyntax node)
        {
            var condition = node.Condition is BinaryExpressionSyntax binary
                ? SyntaxFactory.ParenthesizedExpression(binary)
                : node.Condition;

            return SyntaxFactory.InvocationExpression(
                SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    condition,
                    SyntaxFactory.IdentifierName("Match")
                ),
                argumentList: SyntaxFactory.ArgumentList(
                    SyntaxFactory.Token(SyntaxKind.OpenParenToken),
                    SyntaxFactory.SeparatedList(new[]
                    {
                        SyntaxFactory.Argument(SyntaxFactory.SimpleLambdaExpression(
                            parameter: SyntaxFactory.Parameter(SyntaxFactory.Identifier("t")),
                            body: node.WhenTrue
                        )),
                        SyntaxFactory.Argument(SyntaxFactory.SimpleLambdaExpression(
                            parameter: SyntaxFactory.Parameter(SyntaxFactory.Identifier("f")),
                            body: node.WhenFalse
                        ))
                    }),
                    SyntaxFactory.Token(SyntaxKind.CloseParenToken)
                )
            );
        }
    }
}
