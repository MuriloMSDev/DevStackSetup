# DevStackManager

![DevStackManager Banner](https://img.shields.io/badge/PowerShell-DevStack-blue?style=for-the-badge&logo=powershell)

> **Ambiente de desenvolvimento local completo para Windows, com um só comando.**

---

## 🚀 O que é?
Scripts PowerShell para instalar, gerenciar e remover rapidamente um ambiente de desenvolvimento local moderno (PHP, Node.js, Python, Nginx, MySQL, Composer, phpMyAdmin, MongoDB, Redis, PostgreSQL, Docker, e mais) no Windows.

---

## ⚡ Como usar

Abra um terminal **PowerShell como administrador** e execute:

```powershell
# Torne o DevStackManager global (execute uma vez):
./setup.ps1 global
# Após isso, use 'devstack' ou 'setup.ps1' de qualquer pasta no terminal!
```

### Exemplos de comandos

| Comando                                    | Descrição                                 |
|--------------------------------------------|-------------------------------------------|
| `devstack list php`                        | Listar versões disponíveis do PHP          |
| `devstack install php-8.3.21 nginx mysql`  | Instalar componentes                      |
| `devstack uninstall php-8.3.21 nginx`      | Remover componentes                       |
| `devstack path`                            | Adicionar diretórios ao PATH               |
| `devstack site meuprojeto.localhost ...`   | Criar site Nginx                          |
| `devstack start nginx 1.25.4`              | Iniciar serviço                           |
| `devstack stop php 8.3.21`                 | Parar serviço                             |
| `devstack restart nginx 1.25.4`            | Reiniciar serviço                         |
| `devstack status`                          | Verificar status dos componentes          |
| `devstack test`                            | Testar funcionamento dos binários         |
| `devstack deps`                            | Verificar dependências do sistema         |
| `devstack update php nodejs ...`           | Atualizar para a última versão            |
| `devstack alias php 8.3.21`                | Criar alias/batch para um executável      |
| `devstack logs`                            | Ver logs do DevStack                      |
| `devstack backup`                          | Fazer backup das configs e logs           |
| `devstack clean`                           | Limpar arquivos temporários e logs        |
| `devstack doctor`                          | Diagnóstico do ambiente                   |

---

## 🛠️ Troubleshooting

- Execute sempre como **administrador** para garantir permissões de PATH e hosts.
- Se um download falhar, tente novamente ou verifique sua conexão.
- O arquivo de log `C:\devstack\devstack.log` registra todas as operações.
- Se PATH não atualizar, reinicie o terminal.
- Se o alias 'devstack' não funcionar, feche e abra o PowerShell novamente ou rode `& $PROFILE` para recarregar o perfil.

---

## 🧩 Como estender

- Adicione novos scripts em PowerShell para outros stacks.
- Use as funções helper para evitar duplicação.
- Adicione testes automatizados com [Pester](https://pester.dev/).

---

## 🤝 Contribuição

- Siga o padrão modular dos scripts (tudo em `src/`).
- Adicione exemplos de uso ao README.
- Faça PRs com testes automatizados.
- Sugestões e issues são bem-vindos!

---

## 📂 Estrutura do Projeto

```
DevStackManager/
│   setup.ps1
│   README.md
│
├───src/
│       install.ps1
│       uninstall.ps1
│       path.ps1
│       list.ps1
│       process.ps1
│
├───configs/
│   ├───nginx/
│   ├───php/
│   └───...
└───...
```

---

## 💡 Dica

> Use `devstack doctor` para checar rapidamente se tudo está funcionando!

---

## Licença

MIT