// Licensed to DocFX Companion Tools and contributors under one or more agreements.
// DocFX Companion Tools and contributors licenses this file to you under the MIT license.

namespace DocFXLanguageGenerator
{
    using System.IO;
    using Markdig.Renderers;
    using Markdig.Syntax;

    /// <summary>
    /// A Text Renderer MArkdown class used to replace the original text with the translated text
    /// </summary>
    internal class MarkdownTransformRenderer : TextRendererBase<MarkdownTransformRenderer>
    {
        /// <summary>
        /// Gets or sets the original Markdown.
        /// </summary>
        public string OriginalMarkdown { get; internal set; }

        /// <summary>
        /// Gets or sets where we are in terms of writting.
        /// </summary>
        public int LastWrittenIndex { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MarkdownTransformRenderer"/> class.
        /// </summary>
        /// <param name="writer">The text writer</param>
        /// <param name="originalMarkdown">The original Markdown string</param>
        public MarkdownTransformRenderer(TextWriter writer, string originalMarkdown)
            : base(writer)
        {
            this.OriginalMarkdown = originalMarkdown;
            this.ObjectRenderers.Add(new ContainerBlockRenderer());
            this.ObjectRenderers.Add(new LeafBlockRenderer());
        }

        /// <summary>
        /// Take the next block.
        /// </summary>
        /// <param name="length">The length</param>
        /// <returns>The block text</returns>
        public string TakeNext(int length)
        {
            if (length == 0)
            {
                return null;
            }

            var result = this.OriginalMarkdown.Substring(this.LastWrittenIndex, length);
            this.LastWrittenIndex += length;
            return result;
        }

        /// <summary>
        /// Container Block Renderer.
        /// </summary>
        internal class ContainerBlockRenderer : MarkdownObjectRenderer<MarkdownTransformRenderer, ContainerBlock>
        {
            /// <inheritdoc/>
            protected override void Write(MarkdownTransformRenderer renderer, ContainerBlock obj)
            {
                renderer.WriteChildren(obj);
            }
        }

        /// <summary>
        /// Leaf Block Renderer.
        /// </summary>
        internal class LeafBlockRenderer : MarkdownObjectRenderer<MarkdownTransformRenderer, LeafBlock>
        {
            /// <inheritdoc/>
            protected override void Write(MarkdownTransformRenderer renderer, LeafBlock obj)
            {
                renderer.WriteLeafInline(obj);
            }
        }
    }
}
