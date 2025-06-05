CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

CREATE TABLE usuario (
                         user_uuid UUID DEFAULT uuid_generate_v4() PRIMARY KEY,
                         email VARCHAR(255) NOT NULL UNIQUE,
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
                        imposto_perc DECIMAL(5, 2) NOT NULL,
                        lucro_total DECIMAL(10, 2) DEFAULT 0.0,
                        tipo_ativo VARCHAR(50) NOT NULL
);

CREATE TABLE imovel_arrendado (
                                  imovel_uuid UUID DEFAULT uuid_generate_v4() PRIMARY KEY,
                                  ativo_uuid UUID NOT NULL REFERENCES ativos(ativo_uuid),
                                  designacao VARCHAR(255) NOT NULL,
                                  localizacao VARCHAR(255) NOT NULL,
                                  valor_imovel DECIMAL(10, 2) NOT NULL,
                                  valor_renda DECIMAL(10, 2) NOT NULL,
                                  valor_condominio DECIMAL(10, 2) NOT NULL,
                                  despesas_anuais DECIMAL(10, 2) NOT NULL
);

CREATE TABLE fundo_investimento (
                                    fundo_uuid UUID DEFAULT uuid_generate_v4() PRIMARY KEY,
                                    ativo_uuid UUID NOT NULL REFERENCES ativos(ativo_uuid),
                                    monte_investido DECIMAL(10, 2) NOT NULL,
                                    taxa_juros_padrao DECIMAL(5, 2) NOT NULL
);

CREATE TABLE deposito_prazo (
                                deposito_uuid UUID DEFAULT uuid_generate_v4() PRIMARY KEY,
                                ativo_uuid UUID NOT NULL REFERENCES ativos(ativo_uuid),
                                banco VARCHAR(255) NOT NULL,
                                numero_conta VARCHAR(255) NOT NULL,
                                titulares VARCHAR(255),
                                taxa_juros_anual DECIMAL(5, 2) NOT NULL,
                                valor_inicial DECIMAL(10, 2) NOT NULL
);


CREATE TABLE relatorio (
                           relatorio_uuid UUID DEFAULT uuid_generate_v4() PRIMARY KEY,
                           user_uuid UUID NOT NULL REFERENCES usuario(user_uuid),
                           data_inicio DATE NOT NULL,
                           data_fim DATE,
                           tipo_relatorio VARCHAR(50) NOT NULL
);


CREATE TABLE relatorio_ativos (
                                  relatorio_uuid UUID NOT NULL,
                                  ativo_uuid UUID NOT NULL,
                                  PRIMARY KEY (relatorio_uuid, ativo_uuid),
                                  FOREIGN KEY (relatorio_uuid) REFERENCES relatorio(relatorio_uuid),
                                  FOREIGN KEY (ativo_uuid) REFERENCES ativos(ativo_uuid)
);

INSERT INTO usuario (user_uuid, email, senha, username, tipo_perfil)
VALUES (
           uuid_generate_v4(),
           'admin@ativosfinanceiros.com',
           'CW8nmnsQ91RTNDKKUbomRut//WcG+cJedQ82oyKK1WI=', -- Base64 for 'admin123'
           'Administrador',
           'ADMINISTRADOR'
       );