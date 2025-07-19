![DevStackManager Banner](https://img.shields.io/badge/Build-v2.3.0-blue?style=for-the-badge&logo=build)

<div align="center">
    <img src="src/Shared/DevStack.ico" alt="DevStack Icon" width="100" height="100"/>
</div>
<h1 align="center"><b>DevStack Manager</b></h1><br>


## 🚀 O que é?
**Gerencie rapidamente um ambiente de desenvolvimento local moderno (PHP, Node.js, Python, Nginx, MySQL, Composer, phpMyAdmin, MongoDB, PostgreSQL, e mais) no Windows.**

---

## 📥 Como Instalar

* **Baixe e execute o instalador (recomendado):** [DevStack-2.3.0-Installer.exe](https://github.com/MuriloMSDev/DevStackManager/releases/tag/v2.3.0)

<div align="center">
    <span>━━━ <b>OU</b> ━━━</span>
</div>

* **Clone o repositório e acesse os executáveis:**
    ```
    git clone https://github.com/MuriloMSDev/DevStackManager.git
    cd DevStackManager/release
    ```
    Os executáveis `DevStack.exe` (CLI) e `DevStackGUI.exe` (interface gráfica) estarão disponíveis na pasta `release`.

---

## ⚡ Como Compilar

* **Compile o projeto:**
    ```
    .\scripts\build.ps1 [-WithInstaller] [-Clean]
    cd release
    ```
    Os executáveis `DevStack.exe` (CLI) e `DevStackGUI.exe` (interface gráfica) estarão disponíveis na pasta `release`.

---

### Comandos Disponíveis (usando CLI)

| Comando                                                       | Descrição                                              |
|---------------------------------------------------------------|--------------------------------------------------------|
| `.\DevStack.exe`                                              | Abre um shell interativo (REPL)                        |
| `.\DevStack.exe install <componente> [versão]`                | Instala uma ferramenta ou versão específica            |
| `.\DevStack.exe uninstall <componente> [versão]`              | Remove uma ferramenta ou versão específica             |
| `.\DevStack.exe list <componente\|--installed>`               | Lista versões disponíveis ou instaladas                |
| `.\DevStack.exe path [add\|remove\|list\|help]`               | Gerencia PATH das ferramentas instaladas               |
| `.\DevStack.exe status`                                       | Mostra status de todas as ferramentas                  |
| `.\DevStack.exe test`                                         | Testa todas as ferramentas instaladas                  |
| `.\DevStack.exe update <componente>`                          | Atualiza uma ferramenta para a última versão           |
| `.\DevStack.exe deps`                                         | Verifica dependências do sistema                       |
| `.\DevStack.exe alias <componente> <versão>`                  | Cria um alias .bat para a versão da ferramenta         |
| `.\DevStack.exe global`                                       | Adiciona DevStack ao PATH e cria alias global          |
| `.\DevStack.exe self-update`                                  | Atualiza o DevStackManager                             |
| `.\DevStack.exe clean`                                        | Remove logs e arquivos temporários                     |
| `.\DevStack.exe backup`                                       | Cria backup das configs e logs                         |
| `.\DevStack.exe logs`                                         | Exibe as últimas linhas do log                         |
| `.\DevStack.exe enable <serviço>`                             | Ativa um serviço do Windows                            |
| `.\DevStack.exe disable <serviço>`                            | Desativa um serviço do Windows                         |
| `.\DevStack.exe config`                                       | Abre o diretório de configuração                       |
| `.\DevStack.exe reset <componente>`                           | Remove e reinstala uma ferramenta                      |
| `.\DevStack.exe proxy [set <url>\|unset\|show]`               | Gerencia variáveis de proxy                            |
| `.\DevStack.exe ssl <domínio> [-openssl <versão>]`            | Gera certificado SSL autoassinado                      |
| `.\DevStack.exe db <mysql\|pgsql\|mongo> <comando> [args...]` | Gerencia bancos de dados básicos                       |
| `.\DevStack.exe service`                                      | Lista serviços DevStack (Windows)                      |
| `.\DevStack.exe doctor`                                       | Diagnóstico do ambiente DevStack                       |
| `.\DevStack.exe site <domínio> [opções]`                      | Cria configuração de site nginx                        |
| `.\DevStack.exe help`                                         | Exibe esta ajuda                                       |

---

## 🛠️ Troubleshooting

- Execute sempre como **administrador** para garantir permissões de PATH e hosts.
- Se um download falhar, tente novamente ou verifique sua conexão.
- O arquivo de log `C:\devstack\devstack.log` registra todas as operações.
- Se PATH não atualizar, reinicie o terminal.

---

## 🧩 Como estender

- Adicione novos componentes ou integrações criando código C# nas áreas CLI, GUI ou Shared.
- Use as funções helper para evitar duplicação.

---

## 🤝 Contribuição

- Siga o padrão modular do código (separação CLI/GUI/Shared).
- Adicione exemplos de uso ao README.
- Faça PRs com testes automatizados.
- Sugestões e issues são bem-vindos!

---

## 📂 Estrutura do Projeto

```text
DevStackManager/
│   README.md
│   VERSION
│
├───src/
│   ├───CLI/                   # Projeto da interface de linha de comando
│   │       DevStackCLI.csproj
│   │       Program.cs
│   │       ...
│   │
│   ├───GUI/                   # Projeto da interface gráfica
│   │       DevStackGUI.csproj
│   │       Program.cs
│   │       Gui*.cs
│   │       ...
│   │
│   ├───INSTALLER/             # Projeto do instalador
│   │       DevStackInstaller.csproj
│   │       Program.cs
│   │       app.manifest
│   │       ...
│   │
│   ├───UNINSTALLER/           # Projeto do desinstalador
│   │       DevStackUninstaller.csproj
│   │       Program.cs
│   │       app.manifest
│   │       ...
│   │
│   └───Shared/                # Código compartilhado
│           DevStackConfig.cs
│           DataManager.cs
│           InstallManager.cs
│           UninstallManager.cs
│           ListManager.cs
│           PathManager.cs
│           ProcessManager.cs
│           DevStack.ico
│           AvailableVersions/
│           Components/
│           Models/
│           ...
│
├───scripts/
│       build.ps1                  # Script de compilação
│       build-installer.ps1         # Script de build do instalador
│       build-uninstaller.ps1       # Script de build do desinstalador
│
└───release/                   # Pasta de distribuição
        configs/               # Configurações (nginx, php, etc.)
        DevStack.exe           # CLI compilado
        DevStackGUI.exe        # GUI compilado
        ...
```

---

## 💡 Dica

> Use `.\DevStack.exe doctor` para checar rapidamente se tudo está funcionando!

---

## Licença

MIT