import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';

@Component({
  selector: 'check-row',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './check-row.component.html',
  styleUrl: './check-row.component.css',
})
export class CheckRowComponent {
  @Input() title?: string;
  @Input() className = '';
}
