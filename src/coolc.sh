# Incluya aquí las instrucciones necesarias para ejecutar su compilador

#INPUT_FILE=$1
#OUTPUT_FILE=${INPUT_FILE:0: -2}mips
dotnet CoolMIPS_Compiler_DW/bin/Debug/netcoreapp2.1/CoolMIPS_Compiler_DW.dll $1

# Si su compilador no lo hace ya, aquí puede imprimir la información de contacto
#echo "LINEA_CON_NOMBRE_Y_VERSION_DEL_COMPILADOR"   # Recuerde cambiar estas
#echo "Copyright (c) 2019: Nombre1, Nombre2, Nombre3"    # líneas a los valores correctos

# Llamar al compilador
# echo "Compiling $INPUT_FILE into $OUTPUT_FILE"

