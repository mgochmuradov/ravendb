﻿import resource = require("models/resource");

class filesystem extends resource {
    isDefault = false;
    statistics = ko.observable<filesystemStatisticsDto>();    
    files = ko.observableArray<filesystemFileHeaderDto>();
    disabled = ko.observable<boolean>(false);

    constructor(public name: string, private isDisabled?: boolean) {
        super(name, 'filesystem');
        this.disabled(isDisabled);
        this.itemCount = ko.computed(() => this.statistics() ? this.statistics().FileCount : 0);
    }

    activate() {
        ko.postbox.publish("ActivateFilesystem", this);
    }

    static getNameFromUrl(url: string) {
        var index = url.indexOf("filesystems/");
        return (index > 0) ? url.substring(index + 10) : "";
    }
}
export = filesystem;