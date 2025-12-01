import { NgModule, Optional, SkipSelf } from '@angular/core';
import { ApiModule } from './api/api.module';

@NgModule({
  imports: [ApiModule],
})
export class CoreModule {
  constructor(@Optional() @SkipSelf() parent: CoreModule) {
    if (parent) {
      throw new Error('CoreModule should only be imported in AppModule.');
    }
  }
}
