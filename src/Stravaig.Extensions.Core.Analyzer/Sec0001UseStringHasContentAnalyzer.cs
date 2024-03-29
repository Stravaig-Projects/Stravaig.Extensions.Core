﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace Stravaig.Extensions.Core.Analyzer;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class Sec0001UseStringHasContentAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "SEC0001";

    private static readonly DiagnosticDescriptor Rule =
        new(DiagnosticId,
            Localise.Resource(nameof(Resources.SEC0001_AnalyzerTitle)),
            Localise.Resource(nameof(Resources.SEC0001_MessageFormat)),
            Category.Readability,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: Localise.Resource(nameof(Resources.SEC0001_Description)));

    private static readonly ImmutableArray<DiagnosticDescriptor> CachedSupportedDiagnostics =
        ImmutableArray.Create(Rule);
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        CachedSupportedDiagnostics;

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.SimpleMemberAccessExpression);
    }

    private void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        var simpleMemberAccessExpression = (MemberAccessExpressionSyntax)context.Node;
        if (!simpleMemberAccessExpression.Name.Identifier.Text.Equals(nameof(string.IsNullOrWhiteSpace), StringComparison.Ordinal))
            return;

        // Is it invoked on the System.String (string) class?
        var typeNode = simpleMemberAccessExpression.ChildNodes().FirstOrDefault();
        switch (typeNode)
        {
            case null:
                return;
            case ThisExpressionSyntax:
                return;
            case PredefinedTypeSyntax predefinedType
                when !predefinedType.Keyword.Text.Equals("string", StringComparison.Ordinal):
                return;
            case IdentifierNameSyntax identifierName
                when !identifierName.Identifier.Text.Equals(nameof(String), StringComparison.Ordinal):
                return;
        }

        var invocationExpression = (InvocationExpressionSyntax)simpleMemberAccessExpression.Parent;
        if (invocationExpression == null)
            return;
        var expressionNode = invocationExpression.Parent;
        if (expressionNode == null)
            return;
        var expressionKind = expressionNode.Kind();
        if (expressionKind is not (SyntaxKind.LogicalNotExpression or SyntaxKind.EqualsExpression))
            return;

        // ReSharper disable once SimplifyLinqExpressionUseAll
        if (expressionKind == SyntaxKind.EqualsExpression && 
            !expressionNode.ChildNodes().Any(n => n.IsKind(SyntaxKind.FalseLiteralExpression)))
        {
            return;
        }

        if (invocationExpression.ArgumentList.Arguments.Count == 0)
            return; // Code is incomplete.

        var argument = invocationExpression.ArgumentList.Arguments.First();
        var argText = argument.GetText().ToString();
        var diagnostic = Diagnostic.Create(Rule, expressionNode.GetLocation(), argText);
        context.ReportDiagnostic(diagnostic);
    }
}