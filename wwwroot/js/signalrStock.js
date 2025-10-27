window.iniciarConexionStock = async function (dotnetHelper) {
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/stockHub")
        .build();

    connection.on("ActualizarStock", (productoId, nuevaCantidad) => {
        dotnetHelper.invokeMethodAsync("ActualizarStock", productoId, nuevaCantidad);
    });

    await connection.start();
};
