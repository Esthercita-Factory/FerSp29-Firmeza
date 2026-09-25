-- Script de inicialización de Base de Datos para Firmeza (Database-First con UUID / GUID)
-- Tablas: clientes, productos, ventas, detalles

DROP TABLE IF EXISTS detalles CASCADE;
DROP TABLE IF EXISTS ventas CASCADE;
DROP TABLE IF EXISTS productos CASCADE;
DROP TABLE IF EXISTS clientes CASCADE;

CREATE TABLE IF NOT EXISTS clientes (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    nombre VARCHAR(150) NOT NULL,
    correo VARCHAR(100) NOT NULL UNIQUE,
    telefono VARCHAR(30),
    direccion VARCHAR(200),
    fecha_registro TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS productos (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    codigo VARCHAR(50) NOT NULL UNIQUE,
    nombre VARCHAR(150) NOT NULL,
    descripcion TEXT,
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
    estado VARCHAR(50) NOT NULL DEFAULT 'Completada',
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

-- Datos semilla con identificadores UUID
INSERT INTO clientes (id, nombre, correo, telefono, direccion) VALUES
('a0000000-0000-0000-0000-000000000001', 'Juan Pérez', 'juan.perez@firmeza.com', '3001234567', 'Calle 10 # 20-30'),
('a0000000-0000-0000-0000-000000000002', 'María Gómez', 'maria.gomez@firmeza.com', '3109876543', 'Carrera 15 # 45-67')
ON CONFLICT (id) DO NOTHING;

INSERT INTO productos (id, codigo, nombre, descripcion, precio, stock, activo) VALUES
('b0000000-0000-0000-0000-000000000001', 'PROD-001', 'Cemento Gris 50kg', 'Saco de cemento de alta resistencia', 32000.00, 100, true),
('b0000000-0000-0000-0000-000000000002', 'PROD-002', 'Ladrillo Estructural', 'Ladrillo de arcilla prensado', 1800.00, 5000, true),
('b0000000-0000-0000-0000-000000000003', 'PROD-003', 'Varilla de Acero 1/2 pulgada', 'Varilla corrugada de acero estructural', 45000.00, 200, true)
ON CONFLICT (id) DO NOTHING;

-- Registro de Migración Inicial de EF Core
CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL PRIMARY KEY,
    "ProductVersion" character varying(32) NOT NULL
);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260925003553_InitialCreate', '10.0.12')
ON CONFLICT ("MigrationId") DO NOTHING;
