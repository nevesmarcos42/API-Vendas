-- Script de inicialização do PostgreSQL
-- Cria os bancos de dados necessários para os microserviços

-- Banco de dados para o serviço de vendas
CREATE DATABASE "VendasDB";

-- Banco de dados para o serviço de estoque
CREATE DATABASE "EstoqueDB";

-- Conecta ao banco VendasDB para criar estruturas iniciais
\c "VendasDB";

-- Cria extensões necessárias
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Conecta ao banco EstoqueDB para criar estruturas iniciais
\c "EstoqueDB";

-- Cria extensões necessárias
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";