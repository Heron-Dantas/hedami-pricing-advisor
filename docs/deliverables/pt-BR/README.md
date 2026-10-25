# Guia pessoal em português do Brasil

Leia o [Word em português](HEDAMI-Pricing-Advisor-Guia-Pessoal-pt-BR.docx). Esta edição é material de estudo pessoal de Heron Dantas e ainda não é a apresentação final do projeto.

A versão mantém os 22 capítulos, seis diagramas traduzidos, 22 tabelas e 17 trechos reais. Classes, métodos, caminhos e comandos permanecem em inglês para corresponderem ao código. A explicação dos procedimentos de agentes foi traduzida para estudo, sem alterar os arquivos usados pelo runtime.

A edição inglesa foi preservada byte a byte. A validação passou em 18 verificações, incluindo estrutura do Word, navegação, idioma pt-BR, valores, hashes e identidade dos blocos de código. A montagem dos seis diagramas foi inspecionada; não foi feita renderização completa das páginas do Word.

Arquivos de apoio: [fonte editável](guia-tecnico-fonte.md), [texto expandido](guia-tecnico-completo.md), [validação](validacao-documento.json) e [fotografia das fontes](source-snapshot.json).

Para gerar novamente a partir da raiz, usando as dependências da edição inglesa:

```sh
python3 scripts/build-documentation-pt-br.py
```

Nenhuma chamada paga à API é necessária. O gerador reutiliza definições de formatação locais sem executar a geração inglesa.
