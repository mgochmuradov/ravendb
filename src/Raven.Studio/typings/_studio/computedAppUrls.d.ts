// Interface
interface computedAppUrls {
    adminSettingsCluster: KnockoutComputed<string>;

    databases: KnockoutComputed<string>;
    serverDashboard: KnockoutComputed<string>;
    manageDatabaseGroup: KnockoutComputed<string>;
    clientConfiguration: KnockoutComputed<string>;
    documents: KnockoutComputed<string>;
    revisionsBin: KnockoutComputed<string>;
    conflicts: KnockoutComputed<string>;
    patch: KnockoutComputed<string>;
    indexes: KnockoutComputed<string>;
    newIndex: KnockoutComputed<string>;
    editIndex: (indexName?: string) => KnockoutComputed<string>;
    editExternalReplication: (taskId?: number) => KnockoutComputed<string>;
    editPeriodicBackupTask: (taskId?: number) => KnockoutComputed<string>;
    editSubscription: (taskId?: number, taskName?: string) => KnockoutComputed<string>;
    editRavenEtl: (taskId?: number, taskName?: string) => KnockoutComputed<string>;
    editSqlEtl: (taskId?: number, taskName?: string) => KnockoutComputed<string>;
    query: (indexName?: string) => KnockoutComputed<string>;
    terms: (indexName?: string) => KnockoutComputed<string>;
    importDatabaseFromFileUrl: KnockoutComputed<string>;
    importCollectionFromCsv: KnockoutComputed<string>;
    exportDatabaseUrl: KnockoutComputed<string>;
    migrateDatabaseUrl: KnockoutComputed<string>;
    sampleDataUrl: KnockoutComputed<string>;
    ongoingTasksUrl: KnockoutComputed<string>;
    editExternalReplicationTaskUrl: KnockoutComputed<string>;
    editSubscriptionTaskUrl: KnockoutComputed<string>;
    editRavenEtlTaskUrl: KnockoutComputed<string>;
    editSqlEtlTaskUrl: KnockoutComputed<string>;
    csvImportUrl: KnockoutComputed<string>;
    status: KnockoutComputed<string>;
    indexPerformance: KnockoutComputed<string>;
    settings: KnockoutComputed<string>;
    indexErrors: KnockoutComputed<string>;
    replicationStats: KnockoutComputed<string>;
    visualizer: KnockoutComputed<string>;
    databaseRecord: KnockoutComputed<string>;
    revisions: KnockoutComputed<string>;
    expiration: KnockoutComputed<string>;
    connectionStrings: KnockoutComputed<string>;
    conflictResolution: KnockoutComputed<string>;

    about: KnockoutComputed<string>;

    ioStats: KnockoutComputed<string>;

    statusStorageReport: KnockoutComputed<string>;
    isAreaActive: (routeRoot: string) => KnockoutComputed<boolean>;
    isActive: (routeTitle: string) => KnockoutComputed<boolean>;
    databasesManagement: KnockoutComputed<string>;
}
