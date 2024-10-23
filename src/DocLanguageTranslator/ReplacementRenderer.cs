// Licensed to DocFX Companion Tools and contributors under one or more agreements.
// DocFX Companion Tools and contributors licenses this file to you under the MIT license.

namespace DocFXLanguageGenerator
{
    using System;
    using System.IO;
    using Markdig.Renderers;
    using Markdig.Syntax.Inlines;

    /// <summary>
    /// Replacement Renderer will allow to replace one text by another.
    /// </summary>
    internal class ReplacementRenderer : MarkdownTransformRenderer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReplacementRenderer"/> class.
        /// </summary>
        /// <param name="writer">A text writer.</param>
        /// <param name="originalMarkdown">The original Markdown file.</param>
        /// <param name="func">The transformation function, value as parameters and returning the transformed string.</param>
        public ReplacementRenderer(TextWriter writer, string originalMarkdown, Func<string, string> func)
            : base(writer, originalMarkdown)
        {
            this.ObjectRenderers.Add(new ContainerInlineRenderer(func));
        }

        /// <summary>
        /// Container Inline Renderer.
        /// </summary>
        internal class ContainerInlineRenderer : MarkdownObjectRenderer<ReplacementRenderer, ContainerInline>
        {
            private readonly Func<string, string> function;

            /// <summary>
            /// Initializes a new instance of the <see cref="ContainerInlineRenderer"/> class.
            /// </summary>
            /// <param name="func">The transformation function.</param>
            public ContainerInlineRenderer(Func<string, string> func)
            {
                this.function = func;
            }

            /// <inheritdoc/>
            protected override void Write(ReplacementRenderer renderer, ContainerInline obj)
            {
                if (obj.LastChild == null)
                {
                    return;
                }

                var startIndex = obj.Span.Start;

                // Make sure we flush all previous markdown before rendering this inline entry.
                renderer.Write(renderer.TakeNext(startIndex - renderer.LastWrittenIndex));

                var originalMarkdown = renderer.TakeNext(obj.LastChild.Span.End + 1 - startIndex);
                var newMarkdown = this.function(originalMarkdown);

                renderer.Write(newMarkdown);
            }
        }
    }
}
