CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

CREATE TABLE usuario (
                         user_uuid UUID DEFAULT uuid_generate_v4() PRIMARY KEY,
                         email VARCHAR(255) NOT NULL,
                         senha VARCHAR(255) NOT NULL,
                         username VARCHAR(255) NOT NULL,
                         tipo_perfil VARCHAR(50)
);


CREATE TABLE ativos (
                        ativo_uuid UUID DEFAULT uuid_generate_v4() PRIMARY KEY,
                        user_uuid UUID NOT NULL REFERENCES usuario(user_uuid),
                        nome VARCHAR(255) NOT NULL,
                        data_inicio DATE NOT NULL,
                        duracao_meses INTEGER NOT NULL,
                        valor_inicial DECIMAL(10, 2) NOT NULL,
                        imposto_perc DECIMAL(5, 2),
                        lucro_total DECIMAL(10, 2),
                        tipo_ativo VARCHAR(50)
);


CREATE TABLE imovel_arrendado (
                                  imovel_uuid UUID DEFAULT uuid_generate_v4() PRIMARY KEY,
                                  ativo_uuid UUID NOT NULL REFERENCES ativos(ativo_uuid),
                                  localizacao VARCHAR(255) NOT NULL,
                                  valor_imovel DECIMAL(10, 2) NOT NULL,
                                  valor_renda DECIMAL(10, 2) NOT NULL,
                                  valor_condominio DECIMAL(10, 2) NOT NULL,
                                  despesas_anuais DECIMAL(10, 2) NOT NULL
);

CREATE TABLE fundo_investimento (
                                    fundo_uuid UUID DEFAULT uuid_generate_v4() PRIMARY KEY,
                                    ativo_uuid UUID NOT NULL REFERENCES ativos(ativo_uuid),
                                    juros_padrao DECIMAL(5, 2),
                                    juros_mensal DECIMAL(5, 2),
                                    monte_investido DECIMAL(10, 2) NOT NULL
);


CREATE TABLE deposito_prazo (
                                deposito_uuid UUID DEFAULT uuid_generate_v4() PRIMARY KEY,
                                ativo_uuid UUID NOT NULL REFERENCES ativos(ativo_uuid),
                                banco VARCHAR(255) NOT NULL,
                                numero_conta VARCHAR(255) NOT NULL,
                                titulares VARCHAR(255),
                                taxa_anual DECIMAL(5, 2),
                                valor_inicial DECIMAL(10, 2) NOT NULL
);


CREATE TABLE relatorio (
                           relatorio_uuid UUID DEFAULT uuid_generate_v4() PRIMARY KEY,
                           user_uuid UUID NOT NULL REFERENCES usuario(user_uuid),
                           data_inicio DATE NOT NULL,
                           data_fim DATE
);


CREATE TABLE relatorio_ativos (
                                  relatorio_uuid UUID NOT NULL,
                                  ativo_uuid UUID NOT NULL,
                                  PRIMARY KEY (relatorio_uuid, ativo_uuid),
                                  FOREIGN KEY (relatorio_uuid) REFERENCES relatorio(relatorio_uuid),
                                  FOREIGN KEY (ativo_uuid) REFERENCES ativos(ativo_uuid)
);