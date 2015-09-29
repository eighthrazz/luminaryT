(function () {
    // UI components
    var ui = new UI();

    // SignalR Control 
    var control = new Control(ui);

    // give UI components access to SignalR control
    ui.initControlCompEvents(control);
})();
