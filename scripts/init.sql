-- Script de inicialización de Base de Datos para Firmeza (UUID / GUID nativo)
-- Tipos: id UUID con gen_random_uuid(), columnas de texto text (sin character varying)
-- Tablas: clientes, productos, ventas, detalles

DROP TABLE IF EXISTS detalles CASCADE;
DROP TABLE IF EXISTS ventas CASCADE;
DROP TABLE IF EXISTS productos CASCADE;
DROP TABLE IF EXISTS clientes CASCADE;

DROP FUNCTION IF EXISTS random_id() CASCADE;
DROP FUNCTION IF EXISTS random_guid() CASCADE;
DROP DOMAIN IF EXISTS id CASCADE;
DROP DOMAIN IF EXISTS guid CASCADE;

CREATE TABLE IF NOT EXISTS clientes (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    nombre text NOT NULL,
    correo text NOT NULL UNIQUE,
    telefono text,
    direccion text,
    fecha_registro TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS productos (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    codigo text NOT NULL UNIQUE,
    nombre text NOT NULL,
    descripcion text,
    precio NUMERIC(10, 2) NOT NULL,
    stock INT NOT NULL DEFAULT 0,
    activo BOOLEAN NOT NULL DEFAULT TRUE,
    fecha_creacion TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS ventas (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    cliente_id UUID NOT NULL,
    fecha TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    total NUMERIC(10, 2) NOT NULL DEFAULT 0,
    estado text NOT NULL DEFAULT 'Completada',
    CONSTRAINT fk_ventas_cliente FOREIGN KEY (cliente_id) REFERENCES clientes(id) ON DELETE RESTRICT
);

CREATE TABLE IF NOT EXISTS detalles (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    venta_id UUID NOT NULL,
    producto_id UUID NOT NULL,
    cantidad INT NOT NULL,
    precio_unitario NUMERIC(10, 2) NOT NULL,
    subtotal NUMERIC(10, 2) NOT NULL,
    CONSTRAINT fk_detalles_venta FOREIGN KEY (venta_id) REFERENCES ventas(id) ON DELETE CASCADE,
    CONSTRAINT fk_detalles_producto FOREIGN KEY (producto_id) REFERENCES productos(id) ON DELETE RESTRICT
);

-- Índices para optimizar consultas relacionales
CREATE INDEX IF NOT EXISTS ix_ventas_cliente_id ON ventas(cliente_id);
CREATE INDEX IF NOT EXISTS ix_detalles_venta_id ON detalles(venta_id);
CREATE INDEX IF NOT EXISTS ix_detalles_producto_id ON detalles(producto_id);

-- Datos semilla con identificadores UUID aleatorios
INSERT INTO clientes (id, nombre, correo, telefono, direccion) VALUES
('020e6a9a-37d1-45ea-bd00-f0e07a0925e4', 'Juan Pérez', 'juan.perez@firmeza.com', '3001234567', 'Calle 10 # 20-30'),
('d84f5985-33f6-4025-a0cf-0c62e6e115ae', 'María Gómez', 'maria.gomez@firmeza.com', '3109876543', 'Carrera 15 # 45-67')
ON CONFLICT (id) DO NOTHING;

INSERT INTO productos (id, codigo, nombre, descripcion, precio, stock, activo) VALUES
('77f414f7-e430-4b9c-9abf-c2a7b36fa9fd', 'PROD-001', 'Cemento Gris 50kg', 'Saco de cemento de alta resistencia', 32000.00, 100, true),
('5786f59a-9365-4705-9e57-da83a4acd5ec', 'PROD-002', 'Ladrillo Estructural', 'Ladrillo de arcilla prensado', 1800.00, 5000, true),
('5c7e2ea1-ad5f-4838-aae7-684021bb9fa3', 'PROD-003', 'Varilla de Acero 1/2 pulgada', 'Varilla corrugada de acero estructural', 45000.00, 200, true)
ON CONFLICT (id) DO NOTHING;

INSERT INTO ventas (id, cliente_id, total, estado) VALUES
('bf99c5c4-b3f9-440e-95f5-723e344e2bf1', '020e6a9a-37d1-45ea-bd00-f0e07a0925e4', 64000.00, 'Completada')
ON CONFLICT (id) DO NOTHING;

INSERT INTO detalles (id, venta_id, producto_id, cantidad, precio_unitario, subtotal) VALUES
('2a322162-515b-4da0-9416-66f99b677295', 'bf99c5c4-b3f9-440e-95f5-723e344e2bf1', '77f414f7-e430-4b9c-9abf-c2a7b36fa9fd', 2, 32000.00, 64000.00)
ON CONFLICT (id) DO NOTHING;

-- Registro de Historial de Migraciones EF Core (sin character varying)
CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" text NOT NULL PRIMARY KEY,
    "ProductVersion" text NOT NULL
);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES 
('20260925003553_InitialCreate', '10.0.12')
ON CONFLICT ("MigrationId") DO NOTHING;
