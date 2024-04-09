using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Xml;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace PostgreSqlInAString {
    [Export(typeof(IViewTaggerProvider))]
    [ContentType("CSharp")]
    [Name(nameof(StringSqlTaggerProvider))]
    [TagType(typeof(ClassificationTag))]
    internal sealed class StringSqlTaggerProvider: IViewTaggerProvider {
        private static List<ITextView> IgnoredTextViews = new List<ITextView>();  // For small amount of items at all times, List should be faster than HashSet

        [Import]
        internal IClassificationTypeRegistryService ClassificationRegistry { get; set; }

        [Import]
        internal IViewTagAggregatorFactoryService TagAggregatorFactory { get; set; }

        [Import]
        internal ITextDocumentFactoryService TextDocumentFactoryService { get; set; }

        [Import]
        internal SVsServiceProvider ServiceProvider { get; set; }

        public ITagger<T> CreateTagger<T>(ITextView textView, ITextBuffer buffer) where T : ITag {
            if (textView is null) {
                throw new ArgumentNullException(nameof(textView));
            }

            if (buffer is null) {
                throw new ArgumentNullException(nameof(buffer));
            }

            // Only provide highlighting on the surface buffer
            if (textView.TextBuffer != buffer) {
                return null;
            }

            // Skip creating the tagger, if the method was invoked by calling CreateTagAggregator below, to prevent stack overflow.
            if (IgnoredTextViews.Contains(textView)) {
                return null;
            }

            bool getDocumentResult = TextDocumentFactoryService.TryGetTextDocument(buffer, out ITextDocument document); // Active document might not be the same as the document of the buffer.
            PostgreSqlInAStringConfiguration configuration = PostgreSqlInAStringConfiguration.GetDefault();

            ThreadHelper.ThrowIfNotOnUIThread();
            if (getDocumentResult) {
                DTE2 dte = (DTE2)ServiceProvider.GetService(typeof(SDTE));
                ProjectItem projectItem = dte?.Solution?.FindProjectItem(document.FilePath);
                Project project = projectItem?.ContainingProject;
                if (!string.IsNullOrEmpty(project?.FullName)) {
                    GetProjectExtensionsConfiguration(project.FullName, ref configuration);
                }
            } 

            ITagAggregator<IClassificationTag> tagAggregator;
            try {
                IgnoredTextViews.Add(textView);
                tagAggregator = TagAggregatorFactory.CreateTagAggregator<IClassificationTag>(textView);
            } finally {
                IgnoredTextViews.Remove(textView);
            }

            return new StringSqlTagger(ClassificationRegistry, tagAggregator, configuration) as ITagger<T>;
        }

        private static void GetProjectExtensionsConfiguration(string projectFilePath, ref PostgreSqlInAStringConfiguration configuration) {
            XmlDocument doc = new XmlDocument();
            doc.Load(projectFilePath);  // Don't use cached MS Build project file XML because it is not refreshed with changes during session.
            foreach (XmlNode docNode in doc.FirstChild.ChildNodes) {
                if (docNode.Name == "ProjectExtensions") {
                    foreach (XmlNode projectExtensionsNode in docNode.ChildNodes) {
                        if (projectExtensionsNode.Name == "PostgreSqlInAString") {
                            foreach (XmlNode postgreSqlInAStringNode in projectExtensionsNode.ChildNodes) {
                                if (postgreSqlInAStringNode.Name == "Enabled") {
                                    configuration.Enabled = postgreSqlInAStringNode.InnerText.ToLowerInvariant() == "true";
                                }
                            }

                            break;  // <PostgreSqlInAString> element should occur at most once, repetitions will be ignored
                        }
                    }

                    break;  // <ProjectExtensions> element can occur at most once
                }
            }
        }
    }
}
