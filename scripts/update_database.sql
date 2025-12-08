-- =====================================================
-- SCRIPT POST-MIGRACIÓN
-- Ejecutar DESPUÉS de aplicar la migración de EF Core
-- =====================================================

-- 1. CONFIGURACION_SISTEMA: Insertar datos iniciales
-- =====================================================
INSERT INTO configuracion_sistema (clave, valor, descripcion) VALUES
('CUENTA_CAJA_GENERAL', '3', 'ID de la cuenta de Caja General'),
('CUENTA_INGRESOS_VENTA', '13', 'ID de la cuenta de Ingresos por Ventas'),
('CUENTA_ITBIS_POR_PAGAR', '11', 'ID de la cuenta de ITBIS por Pagar'),
('TASA_ITBIS_DEFAULT', '18.00', 'Tasa de ITBIS por defecto (%)'),
('MONEDA_DEFAULT', '1', 'ID de la moneda por defecto (DOP)')
ON CONFLICT (clave) DO NOTHING;

-- 2. TIPOS_DOCUMENTO: Actualizar registros existentes
-- =====================================================
-- Facturas = Débito (aumentan deuda), aplican ITBIS
UPDATE tipos_documento 
SET tipo_movimiento_esperado = 'DB', aplica_itbis = TRUE, tasa_itbis = 18.00
WHERE UPPER(descripcion) LIKE '%FACTURA%' 
   OR UPPER(descripcion) LIKE '%NOTA DE DÉBITO%'
   OR UPPER(descripcion) LIKE '%NOTA DE DEBITO%';

-- Recibos = Crédito (disminuyen deuda), NO aplican ITBIS
UPDATE tipos_documento 
SET tipo_movimiento_esperado = 'CR', aplica_itbis = FALSE, tasa_itbis = 0.00
WHERE UPPER(descripcion) LIKE '%RECIBO%' 
   OR UPPER(descripcion) LIKE '%COBRO%'
   OR UPPER(descripcion) LIKE '%NOTA DE CRÉDITO%'
   OR UPPER(descripcion) LIKE '%NOTA DE CREDITO%';

-- 3. VERIFICACIÓN
-- =====================================================
SELECT '✅ Configuración del Sistema:' AS info;
SELECT clave, valor, descripcion FROM configuracion_sistema;

SELECT '✅ Tipos de Documento actualizados:' AS info;
SELECT id_tipo_documento, descripcion, tipo_movimiento_esperado, aplica_itbis, tasa_itbis 
FROM tipos_documento;
