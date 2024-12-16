// <copyright file="GlobalSuppressions.cs" company="DocFx Companion Tools">
// Copyright (c) DocFx Companion Tools. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.
using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Performance", "CA1848:Use the LoggerMessage delegates", Justification = "No need to optimaze in console app.")]
[assembly: SuppressMessage("Usage", "CA2254:Template should be a static expression", Justification = "No need to optimize in console app.")]
[assembly: SuppressMessage("StyleCop.CSharp.NamingRules", "SA1309:Field names should not begin with underscore", Justification = "Coding style different")]
[assembly: SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1101:Prefix local calls with this", Justification = "We don't want this.")]
[assembly: SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "We will decide case by case.")]
