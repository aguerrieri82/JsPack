using JsPack.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPack.Core
{
    public class JsModuleParser
    {
        static readonly string PUNTUACTORS = "*/+=?%><!-[]{}();&|^.,:";

        static readonly string[] KEYWORDS = new[] { "as", "from", "abstract", "arguments", "await", "boolean", "break", "byte", "case", "catch", "char", "class", "const", "continue", "debugger", "default", "delete", "do", "double", "else", "enum", "eval", "export", "extends", "false", "final", "finally", "float", "for", "function", "goto", "if", "implements", "import", "in", "instanceof", "int", "interface", "let", "long", "native", "new", "null", "package", "private", "protected", "public", "return", "short", "static", "super", "switch", "synchronized", "this", "throw", "throws", "transient", "true", "try", "typeof", "var", "void", "volatile", "while", "with", "yield" };

        Dictionary<string, JsParsedModule> _modules;

        public JsModuleParser()
        {
            _modules = new Dictionary<string, JsParsedModule>();
            ModuleResolver = new MultiModuleResolver();
        }

        public JsParsedModule Resolve(JsModule context, string module)
        {
            var resModule = ModuleResolver.Resolve(new ModuleResolveContext()
            {
                Root = Path.GetDirectoryName(context.Path)
            }, module);

            if (resModule == null)
            {
                Console.WriteLine("Module {0} not resolved", module);
                return null;
            }

            if (_modules.TryGetValue(resModule.Path, out var parsedModule))
            {
                if (File.GetLastWriteTime(resModule.Path) < parsedModule.ParseTime)
                    return parsedModule;
            }

            parsedModule = Parse(resModule.Path);
  
            return parsedModule;
        }

        public void Resolve(JsModule context, JsReference reference)
        {
            if (reference.ResolveStatus == JsReferenceStatus.NotResolved)
                return;

            reference.FromModule = Resolve(context, reference.FromModuleName);

            if (reference.FromModule == null)
                reference.ResolveStatus = JsReferenceStatus.Error;
            else
                reference.ResolveStatus = JsReferenceStatus.Success;
        }

        public void ResolveReferences(JsParsedModule module)
        {
            if ((module.Flags & JsParsedModuleFlags.Resolving) != 0)
                return;

            module.Flags |= JsParsedModuleFlags.Resolving;

            if ((module.Flags & JsParsedModuleFlags.Imports) == 0)
            {
                module.Imports = new Dictionary<string, JsReference>();

                foreach (var import in module.Elements.OfType<ImportElement>())
                {
                    foreach (var importRef in ResolveImport(module, import))
                        module.Imports[importRef.Alias] = importRef;
                }

                module.Flags |= JsParsedModuleFlags.Imports;
            }

            if ((module.Flags & JsParsedModuleFlags.Exports) == 0)
            {
                module.Exports = new Dictionary<string, JsReference>();

                foreach (var export in module.Elements.OfType<ExportElement>())
                {
                    foreach (var exportRef in ResolveExport(module, export))
                        module.Exports[exportRef.Alias] = exportRef;
                }

                module.Flags |= JsParsedModuleFlags.Exports;

            }

            module.Flags ^= JsParsedModuleFlags.Resolving;
        }

        public IEnumerable<JsReference> ResolveImport(JsParsedModule module, ImportElement import)
        {
            var impModule = Resolve(module, import.FromModule);

            if (impModule == null)
                yield break;

            ResolveReferences(impModule);

            if (import.AllAlias != null)
            {
     
            }
        }

        public IEnumerable<JsReference> ResolveExport(JsParsedModule module, ExportElement export)
        {
            if (export.FromModule != null)
            {
                var impModule = Resolve(module, export.FromModule);
                if (impModule == null)
                    yield break;

                ResolveReferences(impModule);

                if (export.Items == null || export.Items.Count == 0)
                {
                    if (export.ExportName != null)
                    {
                        yield return new JsReference()
                        {
                            FromModule = impModule,
                            FromModuleName = impModule.Name,
                            Alias = export.ExportName,
                            Name = "*",
                            ResolveStatus = JsReferenceStatus.Success
                        };
                    }
                    else
                    {
                        foreach (var item in impModule.Exports)
                            yield return item.Value;
                    }
                }
                else
                {
                    foreach (var item in export.Items)
                    {
                        var isFound = impModule.Exports.TryGetValue(item.Identifier, out var exportRef);
                        yield return new JsReference()
                        {
                            FromModule = impModule,
                            FromModuleName = impModule.Name,
                            Alias = item.Alias ?? item.Identifier,
                            Name = item.Identifier,
                            ResolveStatus = isFound ? JsReferenceStatus.Success : JsReferenceStatus.Error
                        };

                        if (!isFound)
                            Console.WriteLine("Re-export reference {0} from module {1} not found", item.Identifier, impModule.Name);
                    }
                }
            }
            else
            {
                if (export.Items != null)
                {
                    foreach (var item in export.Items)
                    {
                        yield return new JsReference()
                        {
                            FromModule = module,
                            FromModuleName = module.Name,
                            Alias = item.Alias ?? item.Identifier,
                            Name = item.Identifier,
                            ResolveStatus = JsReferenceStatus.Success
                        };
                    }
                }
                if (export.ExportName != null || export.IsDefault) {
                    yield return new JsReference()
                    {
                        FromModule = module,
                        FromModuleName = module.Name,
                        Alias = export.IsDefault ? "default" : export.ExportName,
                        Name = export.ExportName,
                        ResolveStatus = JsReferenceStatus.Success
                    };
                }
            }
        }

        public JsParsedModule Parse(string path, bool resolveRefs = false)
        {
            path = Path.GetFullPath(path);

            using var reader = new StreamReader(path);

            var result = new JsParsedModule()
            {
                Name = Path.GetFileNameWithoutExtension(path),
                Path = path,
                Elements = Parse(Tokenize(reader)).ToArray(),
                ParseTime = DateTime.Now,
                Flags= JsParsedModuleFlags.Parsed
            };

            _modules[path] = result;

            if (resolveRefs)
                ResolveReferences(result);

            return result;
        }

        public IEnumerable<JsParsedModule> ParseAll(string path, string filter = "*.js")
        {
            foreach (var file in Directory.GetFiles(path, filter))
                yield return Parse(file);

            foreach (var file in Directory.GetDirectories(path))
            {
                foreach (var item in ParseAll(file, filter))
                    yield return item;
            }
        }

        public IEnumerable<JsElement> Parse(IEnumerable<Token> tokens, Action<InvalidSyntaxException> errorHandler = null)
        {
            var state = 0;
            JsElement curElement = null;
            var enumertaor = tokens.Where(a => a.Type != TokenType.Whitespace && a.Type != TokenType.Comment).GetEnumerator();
            Token token = null;
            var advance = true;

            var InvalidSyntax = () =>
            {
                var ex = new InvalidSyntaxException(token);
                if (errorHandler != null)
                {
                    errorHandler(ex);
                    state = 0;
                    curElement = null;
                }
                else
                    throw ex;
            };

            while (true)
            {
                if (advance)
                {
                    if (!enumertaor.MoveNext())
                        yield break;
                    token = enumertaor.Current;
                }
                advance = true;
                switch (state)
                {
                    case 0:
                        if (token.Type == TokenType.Keyword)
                        {
                            if (token.Text == "export")
                            {
                                curElement = new ExportElement()
                                {
                                    Items = new List<IdentifierAlias>()
                                };
                                state = 1;
                            }
                            else if (token.Text == "import")
                            {
                                curElement = new ImportElement();
                                state = 10;
                            }
                        }
                        else if (token.Type == TokenType.Identifier)
                        {
                            curElement = new IdentifierElement()
                            {
                                Parts = new List<string>(new[] { token.Text })
                            };

                            state = 30;
                        }
                        break;
                    case 1:
                        if (token.Text == "*")
                            state = 3;
                        else if (token.Text == "{")
                            state = 37;
                        else if (token.Text == "async")
                            state = 1;
                        else if (token.Text == "default")
                        {
                            (curElement as ExportElement).IsDefault = true;
                        }
                        else
                        {
                            (curElement as ExportElement).ExportType = token.Text;
                            if (token.Text == "var" || token.Text == "const" || token.Text == "let")
                                state = 20;
                            else
                                state = 41;
                        }

                        break;
                    case 2:
                        if (token.Type == TokenType.Identifier)
                        {
                            (curElement as ExportElement).ExportName = token.Text;
                            yield return curElement;
                            curElement = null;
                        }
                        else
                            InvalidSyntax();
                        break;
                    case 3:
                        if (token.Text == "as")
                            state = 43;
                        else if (token.Text == "from")
                            state = 40;
                        else
                            InvalidSyntax();
                        break;
                    case 10:
                        if (token.Text == "*")
                            state = 11;
                        else if (token.Text == "{")
                        {
                            state = 12;
                            (curElement as ImportElement).Items = new List<IdentifierAlias>();
                        }
                        else if (token.Type == TokenType.Identifier)
                        {
                            (curElement as ImportElement).DefaultAlias = token.Text;
                            state = 17;
                        }
                        else if (token.Type == TokenType.String)
                        {
                            (curElement as ImportElement).FromModule = token.Text;
                            yield return curElement;
                            curElement = null;
                            state = 0;
                        }
                        break;
                    case 11:
                        if (token.Type == TokenType.Keyword && token.Text == "as")
                            state = 14;
                        else
                            InvalidSyntax();
                        break;
                    case 12:
                        if (token.Text[0] == '}')
                            state = 15;
                        else if (token.Type == TokenType.Identifier)
                        {
                            (curElement as ImportElement).Items.Add(new IdentifierAlias()
                            {
                                Identifier = token.Text
                            });
                            state = 13;
                        }
                        break;
                    case 13:
                        if (token.Type == TokenType.Keyword)
                        {
                            if (token.Text == "as")
                                state = 18;
                            else
                                InvalidSyntax();
                        }
                        else
                        {
                            if (token.Text == ",")
                                state = 12;
                            else if (token.Text[0] == '}')
                                state = 15;
                            else
                                InvalidSyntax();
                        }
                        break;
                    case 14:
                        (curElement as ImportElement).AllAlias = token.Text;
                        state = 15;
                        break;
                    case 15:
                        if (token.Type == TokenType.Keyword && token.Text == "from")
                            state = 16;
                        else
                            InvalidSyntax();
                        break;
                    case 16:
                        (curElement as ImportElement).FromModule = token.Text;
                        yield return curElement;
                        curElement = null;
                        state = 0;
                        break;
                    case 17:
                        if (token.Text == ",")
                            state = 10;
                        else if (token.Type == TokenType.Keyword && token.Text == "from")
                            state = 16;
                        break;
                    case 18:
                        if (token.Type == TokenType.Identifier)
                        {
                            (curElement as ImportElement).Items.Last().Alias = token.Text;
                            state = 13;
                        }
                        else
                            state = 0;
                        break;
                    case 20:
                        if (token.Text == ";")
                        {
                            yield return curElement;
                            curElement = null;
                            state = 0;
                        }
                        else if (token.Text == "{" && (curElement as ExportElement).ExportType == "const")
                        {
                            if ((curElement as ExportElement).Items.Count > 0)
                                InvalidSyntax();
                            else
                                state = 22;
                        }
                        else if (token.Type == TokenType.Identifier)
                        {
                            (curElement as ExportElement).Items.Add(new IdentifierAlias() { Identifier = token.Text });
                            state = 27;
                        }
                        break;
                    case 21:
                        if (token.Text == ",")
                            state = 22;
                        else if (token.Text[0] == '}')
                            state = 23;
                        else
                            InvalidSyntax();
                        break;
                    case 22:
                        if (token.Type == TokenType.Identifier)
                        {
                            (curElement as ExportElement).Items.Add(new IdentifierAlias() { Identifier = token.Text });
                            state = 24;
                        }
                        else
                            InvalidSyntax();
                        break;
                    case 23:
                        if (token.Text == "=")
                            state = 26;
                        else
                        {
                            yield return curElement;
                            curElement = null;
                            state = 0;
                        }
                        break;
                    case 24:
                        if (token.Text == ":")
                            state = 25;
                        else 
                        { 
                            state = 21;
                            advance = false;
                        }
                        break;
                    case 25:
                        if (token.Type == TokenType.Identifier)
                        {
                            (curElement as ExportElement).Items.Last().Alias = token.Text;
                            state = 21;
                        }
                        else
                            InvalidSyntax();

                        break;
                    case 26:
                        if (token.Type == TokenType.Identifier)
                        {
                            (curElement as ExportElement).ExportName = token.Text;
                            yield return curElement;
                            curElement = null;
                            state = 0;
                        }
                        else
                            InvalidSyntax();
                        break;
                    case 27:
                        if (token.Text == ";")
                        {
                            if ((curElement as ExportElement).Items.Count == 1)
                            {
                                (curElement as ExportElement).ExportName = (curElement as ExportElement).Items[0].Identifier;
                                (curElement as ExportElement).Items.Clear();                            
                            }
                            yield return curElement;
                            curElement = null;
                            state = 0;
                        }
                        else if (token.Type == TokenType.Identifier)
                            (curElement as ExportElement).Items.Add(new IdentifierAlias() { Identifier = token.Text });

                        break;
                    case 30:
                        if (token.Text == ".")
                            state = 31;
                        else
                        {
                            yield return curElement;
                            curElement = null;
                            state = 0;
                            advance = false;
                        }
                        break;
                    case 31:
                        if (token.Type == TokenType.Identifier || (token.Type == TokenType.Keyword && (curElement as IdentifierElement).Parts.Count > 0))
                        {
                            (curElement as IdentifierElement).Parts.Add(token.Text);
                            state = 30;
                        }
                        else
                            InvalidSyntax();

                        break;
                    case 35:
                        if (token.Text == "{")
                            state = 37;
                        else if (token.Text == "function" || token.Text == "class")
                            state = 36;
                        else
                            InvalidSyntax();
                        break;
                    case 36:
                        if (token.Type == TokenType.Identifier)
                            (curElement as ExportElement).ExportName = token.Text;
                        yield return curElement;
                        curElement = null;
                        state = 0;
                        break;

                    case 37:
                        if (token.Type == TokenType.Identifier)
                        {
                            (curElement as ExportElement).Items.Add(new IdentifierAlias() { Identifier = token.Text });
                            state = 38;
                        }
                        else if (token.Type == TokenType.Keyword && token.Text == "default")
                        {
                            (curElement as ExportElement).IsDefault = true;
                        }
                        else if (token.Text[0] == '}')
                            state = 39;
                        else
                            InvalidSyntax();
                        break;
                    case 38:
                        if (token.Text == "as")
                            state = 42;
                        else if (token.Text == ",")
                            state = 37;
                        else if (token.Text[0] == '}')
                            state = 39;
                        else
                            InvalidSyntax();
                        break;
                    case 39:
                        if (token.Text == "from")
                            state = 40;
                        else
                        {
                            yield return curElement;
                            curElement = null;
                            state = 0;
                            advance = false;
                        }
                        break;
                    case 40:
                        if (token.Type == TokenType.String)
                        {
                            (curElement as ExportElement).FromModule = token.Text;
                            yield return curElement;
                            curElement = null;
                            state = 0;
                        }
                        else
                            InvalidSyntax();
                        break;

                    case 41:
                        if (token.Type == TokenType.Identifier)
                            (curElement as ExportElement).ExportName = token.Text;
                        else
                        {
                            if (!(curElement as ExportElement).IsDefault)
                                InvalidSyntax();
                        }
                        yield return curElement;
                        curElement = null;
                        state = 0;
                        break;
                    case 42:
                        if (token.Type == TokenType.Identifier || (token.Type == TokenType.Keyword && token.Text == "default"))
                        {
                            (curElement as ExportElement).Items.Last().Alias = token.Text;
                            state = 38;
                        }
                        else
                            InvalidSyntax();
                        break;
                    case 43:
                        if (token.Type == TokenType.Identifier) {
                            (curElement as ExportElement).ExportName = token.Text;
                            state = 39;
                        }
                        else
                            InvalidSyntax();
                        break;
                }
            }

        }

        public IEnumerable<Token> Tokenize(TextReader reader)
        {
            var state = 0;
            var curText = new StringBuilder();
            Token curToken = null;
            char c = '\0';
            bool advance = true;
            var line = 0;
            var lineOfs = 0;
            var ofs = -1;
            var waitNewLine = false;
            int blockStack = 0;

            var CreateToken = (TokenType type) =>
            {
                return new Token()
                {
                    Type = type,
                    Position = ofs,
                    Line = line,
                    Column = ofs - lineOfs
                };
            };

            while (true)
            {
                if (advance)
                {
                    c = (char)reader.Read();
                    if (c == '\uffff')
                        yield break;
                    ofs++;

                    if (waitNewLine)
                    {
                        if (c == '\n')
                            lineOfs++;
                        waitNewLine = false;
                    }

                    else if (c == '\r' || c == '\n')
                    {
                        line++;
                        lineOfs = ofs + 1;
                        waitNewLine = c == '\r';
                    }

                }

                advance = true;

                switch (state)
                {
                    case 0:
                        if (c == '"')
                        {
                            state = 1;
                            curToken = CreateToken(TokenType.String);
                        }
                        else if (c == '\'')
                        {
                            state = 4;
                            curToken = CreateToken(TokenType.String);
                        }
                        else if (c == '`')
                        {
                            state = 14;
                            curToken = CreateToken(TokenType.String);
                        }
                        else if (char.IsWhiteSpace(c))
                        {

                            curText.Append(c);
                            curToken = CreateToken(TokenType.Whitespace);
                            state = 3;

                        }
                        else if (char.IsLetter(c) || c == '_' || c == '$')
                        {
                            state = 6;
                            curText.Append(c);
                            curToken = CreateToken(TokenType.Identifier);
                        }
                        else if (char.IsDigit(c))
                        {
                            state = 7;
                            curText.Append(c);
                            curToken = CreateToken(TokenType.Number);
                        }

                        else if (c == '/')
                        {
                            state = 10;
                            curToken = CreateToken(TokenType.Comment);
                        }
                        else if (PUNTUACTORS.Contains(c))
                        {
                            state = 9;
                            curText.Append(c);
                            curToken = CreateToken(TokenType.Puntuacator);
                        }
                        /*
                        else
                            throw new Exception(String.Format("Unspected char '{0}' at {1}", c, ofs));*/
                        break;
                    case 1:
                        if (c == '\\')
                            state = 2;
                        else if (c == '"')
                        {
                            curToken.Text = curText.ToString();
                            yield return curToken;
                            curText.Clear();
                            curToken = null;
                            state = 0;
                        }
                        else
                            curText.Append(c);
                        break;
                    case 2:
                        curText.Append(c);
                        state = 1;
                        break;
                    case 3:
                        if (char.IsWhiteSpace(c))
                            curText.Append(c);
                        else
                        {
                            curToken.Text = curText.ToString();
                            yield return curToken;
                            curText.Clear();
                            curToken = null;
                            advance = false;
                            state = 0;
                        }
                        break;
                    case 4:
                        if (c == '\\')
                            state = 5;
                        else if (c == '\'')
                        {
                            curToken.Text = curText.ToString();
                            yield return curToken;
                            curText.Clear();
                            curToken = null;
                            state = 0;
                        }
                        else
                            curText.Append(c);
                        break;
                    case 5:
                        curText.Append(c);
                        state = 4;
                        break;
                    case 6:
                        if (char.IsLetterOrDigit(c) || char.IsLetterOrDigit(c) || c == '_')
                            curText.Append(c);
                        else
                        {

                            curToken.Text = curText.ToString();
                            if (KEYWORDS.Contains(curToken.Text))
                                curToken.Type = TokenType.Keyword;

                            yield return curToken;
                            curText.Clear();
                            advance = false;
                            curToken = null;
                            state = 0;
                        }
                        break;
                    case 7:
                        if (c == 'x' || c == 'X' || c == '.' || char.IsDigit(c))
                        {
                            curText.Append(c);
                            state = 8;
                        }
                        else
                        {
                            curToken.Text = curText.ToString();
                            yield return curToken;
                            curText.Clear();
                            advance = false;
                            curToken = null;
                            state = 0;
                        }
                        break;
                    case 8:
                        if (char.IsDigit(c))
                            curText.Append(c);
                        else
                        {
                            curToken.Text = curText.ToString();
                            yield return curToken;
                            curText.Clear();
                            advance = false;
                            curToken = null;
                            state = 0;
                        }
                        break;
                    case 9:
                        if (PUNTUACTORS.Contains(c))
                            curText.Append(c);
                        else
                        {
                            curToken.Text = curText.ToString();
                            yield return curToken;
                            curText.Clear();
                            curToken = null;
                            advance = false;
                            state = 0;
                        }
                        break;
                    case 10:
                        if (c == '/')
                            state = 11;
                        else if (c == '*')
                            state = 12;
                        else
                        {
                            curToken = CreateToken(TokenType.Puntuacator);
                            curToken.Text = "/";
                            yield return curToken;
                            curText.Clear();
                            curToken = null;
                            advance = false;
                            state = 0;
                        }
                        break;
                    case 11:
                        if (c == '\n')
                        {
                            curToken.Text = curText.ToString();
                            yield return curToken;
                            curText.Clear();
                            curToken = null;
                            state = 0;
                        }
                        else
                            curText.Append(c);
                        break;
                    case 12:
                        if (c == '*')
                            state = 13;
                        else
                            curText.Append(c);
                        break;
                    case 13:
                        if (c == '/')
                        {
                            curToken.Text = curText.ToString();
                            yield return curToken;
                            curText.Clear();
                            curToken = null;
                            state = 0;
                        }
                        else
                        {
                            curText.Append('*');
                            curText.Append(c);
                            state = 12;
                        }
                        break;
                    case 14:
                        if (c == '\\')
                            state = 15;
                        else if (c == '$')
                            state = 16;
                        else if (c == '`')
                        {
                            curToken.Text = curText.ToString();
                            yield return curToken;
                            curText.Clear();
                            curToken = null;
                            state = 0;
                        }
                        else
                            curText.Append(c);
                        break;
                    case 15:
                        curText.Append(c);
                        state = 14;
                        break;
                    case 16:
                        if (c == '{')
                        {
                            blockStack = 1;
                            state = 17;
                        }
                        else
                        {
                            curText.Append("$");
                            state = 14;
                            advance = false;
                        }
                        break;
                    case 17:
                        if (c == '}')
                        {
                            blockStack--;
                            if (blockStack == 0)
                                state = 14;
                        }
                        else if (c == '{')
                            blockStack++;

                        curText.Append(c);
                        
                        break;
                }
            }
        }


        public IModuleResolver ModuleResolver { get; set; }
    }
}
