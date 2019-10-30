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
    [SyntaxRule(SafeEqualsRule.Name, SafeEqualsRule.Description, SyntaxRuleOrder.SafeEquals)]
    internal sealed class SafeEqualsRule : CSharpOnlyFormattingRule, ISyntaxFormattingRule
    {
        internal const string Name = "SafeEquals";
        internal const string Description = "Replace usages of == with SafeEquals.";

        public SyntaxNode Process(SyntaxNode syntaxRoot, string languageName)
        {
            return new SafeEqualsRewriter().Visit(syntaxRoot);
        }
    }

    internal sealed class SafeEqualsRewriter : CSharpSyntaxRewriter
    {
        public override SyntaxNode VisitEqualsValueClause(EqualsValueClauseSyntax node)
        {
            var binaryExpression = (BinaryExpressionSyntax)node.Value;
            return node.WithValue(SyntaxFactory.InvocationExpression(
                SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    binaryExpression.Left,
                    SyntaxFactory.IdentifierName("SafeEquals")
                ),
                argumentList: SyntaxFactory.ArgumentList(
                    SyntaxFactory.Token(SyntaxKind.OpenParenToken),
                    SyntaxFactory.SeparatedList(new[]
                    {
                        SyntaxFactory.Argument(binaryExpression.Right)
                    }),
                    SyntaxFactory.Token(SyntaxKind.CloseParenToken)
                )
            ));
        }
    }
}
