import { Component, inject, OnInit } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { SessionManagerService } from './core/services/session-manager.service';
import { NgIf } from '@angular/common';
import { NavbarComponent } from './shared/navbar/navbar.component';
import { FooterComponent } from './shared/footer/footer.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, NgIf, NavbarComponent, FooterComponent],
  templateUrl: './app.html',
})
export class App implements OnInit {
  private session = inject(SessionManagerService);
  protected readonly title = 'frontend';

  ngOnInit(): void {
    this.session.restoreSession();
  }

  get loading() {
    return this.session.loading();
  }
}
